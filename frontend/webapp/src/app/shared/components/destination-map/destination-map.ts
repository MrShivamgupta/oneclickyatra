import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, effect, input, output } from '@angular/core';
import * as L from 'leaflet';

export interface MapMarker {
  lat: number;
  lng: number;
  label: string;
  slug?: string;
}

// Esri's free (no API key) World Street Map basemap -- chosen over the plain OpenStreetMap tiles
// because OSM's standard style renders every place name in its own local script/language (Arabic,
// Urdu, Greek, etc.), while Esri's reference layer labels everything in English/Latin script
// globally, matching what a user of this site actually asked for.
const TILE_LAYER_URL = 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}';
// Esri's terms require credit for the free tiles, but not the full multi-vendor legal text their
// REST service metadata publishes -- a short "Esri" credit is the commonly-accepted practical form
// and keeps Leaflet's attribution corner compact instead of wrapping across two lines.
const TILE_ATTRIBUTION = 'Tiles &copy; Esri';

/** Free, no-API-key map (Leaflet + OpenStreetMap tiles) used on the public site to show where a
 * destination (or every destination) actually is. Custom div-icon pins, not Leaflet's default
 * marker images -- those reference relative image paths that don't survive Angular's bundler
 * without extra asset-copying config, so a plain colored dot avoids that well-known gotcha entirely. */
@Component({
  selector: 'app-destination-map',
  standalone: true,
  templateUrl: './destination-map.html',
  styleUrl: './destination-map.scss'
})
export class DestinationMap implements AfterViewInit, OnDestroy {
  readonly markers = input<MapMarker[]>([]);
  readonly height = input('400px');

  readonly markerSelected = output<MapMarker>();

  @ViewChild('mapContainer', { static: true }) private mapContainer!: ElementRef<HTMLDivElement>;

  private map: L.Map | null = null;
  private markerLayer: L.LayerGroup | null = null;
  private tileLayerAdded = false;

  constructor() {
    // Only reacts to markers changing AFTER the initial render below has already set the map's
    // real starting view -- not relevant on first load (the input already holds real data by the
    // time this component exists, since both pages that use it gate it behind `@if (markers.length)`),
    // but keeps the map correct if a parent ever swaps its marker set later.
    effect(() => {
      const markers = this.markers();
      if (this.tileLayerAdded) this.renderMarkers(markers);
    });
  }

  ngAfterViewInit(): void {
    const element = this.mapContainer.nativeElement;
    element.style.height = this.height();

    const valid = this.markers().filter((marker) => marker.lat != null && marker.lng != null);
    const initialView = this.computeView(valid);

    // Set the map's real starting view BEFORE any tile layer exists to react to it. Adding a tile
    // layer immediately fires tile requests for whatever view is current at that moment -- if a
    // fitBounds()/setView() call changed the view again moments later (e.g. once async marker data
    // arrived), Leaflet aborts the first batch mid-flight, leaving tiles from two different zoom
    // levels visibly overlapping. Computing the correct view up front means tiles only ever load once.
    this.map = L.map(element, { ...initialView, scrollWheelZoom: false });

    L.tileLayer(TILE_LAYER_URL, { attribution: TILE_ATTRIBUTION, maxZoom: 18 }).addTo(this.map);
    this.tileLayerAdded = true;

    this.markerLayer = L.layerGroup().addTo(this.map);

    // Scrolling the page shouldn't get hijacked by the map until a visitor deliberately hovers it.
    element.addEventListener('mouseenter', () => this.map?.scrollWheelZoom.enable());
    element.addEventListener('mouseleave', () => this.map?.scrollWheelZoom.disable());

    // Leaflet measures its container's size at creation time; if the surrounding layout (a grid/flex
    // parent) hasn't finished settling by then, tiles render as a fragmented, misaligned mosaic.
    // A follow-up invalidateSize() once the browser has actually painted the layout fixes it.
    requestAnimationFrame(() => this.map?.invalidateSize());

    this.addPins(valid);
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private computeView(markers: MapMarker[]): { center: L.LatLngExpression; zoom: number } {
    if (markers.length === 0) return { center: [20, 0], zoom: 2 };
    if (markers.length === 1) return { center: [markers[0].lat, markers[0].lng], zoom: 9 };

    const bounds = L.latLngBounds(markers.map((marker): [number, number] => [marker.lat, marker.lng]));
    // No map exists yet to ask for a real pixel-based fit, so approximate one from the bounds alone.
    const center = bounds.getCenter();
    const spanDegrees = Math.max(bounds.getNorth() - bounds.getSouth(), bounds.getEast() - bounds.getWest());
    const zoom = spanDegrees > 60 ? 2 : spanDegrees > 30 ? 3 : spanDegrees > 15 ? 4 : spanDegrees > 7 ? 5 : spanDegrees > 3 ? 6 : 7;
    return { center: [center.lat, center.lng], zoom };
  }

  private renderMarkers(markers: MapMarker[]): void {
    if (!this.map) return;
    const valid = markers.filter((marker) => marker.lat != null && marker.lng != null);
    this.addPins(valid);

    if (valid.length === 1) {
      this.map.setView([valid[0].lat, valid[0].lng], 9);
    } else if (valid.length > 1) {
      const bounds = L.latLngBounds(valid.map((marker): [number, number] => [marker.lat, marker.lng]));
      this.map.fitBounds(bounds, { padding: [30, 30] });
    }
  }

  private addPins(markers: MapMarker[]): void {
    if (!this.markerLayer) return;
    this.markerLayer.clearLayers();
    if (markers.length === 0) return;

    const icon = L.divIcon({
      className: 'destination-pin',
      html: '<span class="pin-dot"></span>',
      iconSize: [16, 16],
      iconAnchor: [8, 8]
    });

    for (const marker of markers) {
      const leafletMarker = L.marker([marker.lat, marker.lng], { icon }).bindTooltip(marker.label);
      leafletMarker.on('click', () => this.markerSelected.emit(marker));
      leafletMarker.addTo(this.markerLayer);
    }
  }
}
