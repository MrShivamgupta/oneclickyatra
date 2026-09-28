import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, effect, input, output } from '@angular/core';
import * as L from 'leaflet';
import * as topojson from 'topojson-client';
import worldTopology from 'world-atlas/countries-110m.json';

export interface CountryCount {
  countryName: string;
  count: number;
  lat: number;
  lng: number;
}

/** A stylized "at a glance" overview map -- solid country silhouettes on a flat background, with a
 * numbered badge per country instead of one pin per destination. Deliberately NOT a photographic/
 * street-style map: this is meant as a quick illustrative summary, unlike DestinationMap (used on the
 * Destination Detail page), which shows a real explorable map of one specific place.
 *
 * Country outlines come from `world-atlas` (public-domain Natural Earth data bundled as a small
 * ~108KB static JSON asset, no runtime fetch/API key) converted to GeoJSON via `topojson-client`. */
@Component({
  selector: 'app-region-map',
  standalone: true,
  templateUrl: './region-map.html',
  styleUrl: './region-map.scss'
})
export class RegionMap implements AfterViewInit, OnDestroy {
  readonly countries = input<CountryCount[]>([]);
  readonly height = input('360px');

  readonly countrySelected = output<CountryCount>();

  @ViewChild('mapContainer', { static: true }) private mapContainer!: ElementRef<HTMLDivElement>;

  private map: L.Map | null = null;
  private badgeLayer: L.LayerGroup | null = null;

  constructor() {
    effect(() => {
      const countries = this.countries();
      if (this.map) this.renderBadges(countries);
    });
  }

  ngAfterViewInit(): void {
    const element = this.mapContainer.nativeElement;
    element.style.height = this.height();

    const countries = this.countries();
    const view = this.computeView(countries);

    this.map = L.map(element, {
      ...view,
      zoomControl: false,
      dragging: false,
      scrollWheelZoom: false,
      doubleClickZoom: false,
      boxZoom: false,
      keyboard: false,
      touchZoom: false,
      attributionControl: false
    });

    const countryShapes = topojson.feature(
      worldTopology as unknown as Parameters<typeof topojson.feature>[0],
      (worldTopology as { objects: { countries: Parameters<typeof topojson.feature>[1] } }).objects.countries
    );
    L.geoJSON(countryShapes as GeoJSON.GeoJsonObject, {
      style: () => ({ fillColor: '#ffffff', fillOpacity: 1, color: '#0b7a70', weight: 1 })
    }).addTo(this.map);

    this.badgeLayer = L.layerGroup().addTo(this.map);
    requestAnimationFrame(() => this.map?.invalidateSize());

    this.renderBadges(countries);
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private computeView(countries: CountryCount[]): { center: L.LatLngExpression; zoom: number } {
    if (countries.length === 0) return { center: [15, 100], zoom: 3 };
    if (countries.length === 1) return { center: [countries[0].lat, countries[0].lng], zoom: 4 };

    const bounds = L.latLngBounds(countries.map((c): [number, number] => [c.lat, c.lng]));
    const center = bounds.getCenter();
    const spanDegrees = Math.max(bounds.getNorth() - bounds.getSouth(), bounds.getEast() - bounds.getWest());
    const zoom = spanDegrees > 60 ? 2 : spanDegrees > 30 ? 3 : spanDegrees > 15 ? 4 : spanDegrees > 7 ? 5 : 6;
    return { center: [center.lat, center.lng], zoom };
  }

  private renderBadges(countries: CountryCount[]): void {
    if (!this.badgeLayer) return;
    this.badgeLayer.clearLayers();

    for (const country of countries) {
      const icon = L.divIcon({
        className: 'country-badge',
        html: `<span class="badge-count">${country.count}</span>`,
        iconSize: [34, 34],
        iconAnchor: [17, 17]
      });
      const marker = L.marker([country.lat, country.lng], { icon }).bindTooltip(
        `${country.countryName} (${country.count})`
      );
      marker.on('click', () => this.countrySelected.emit(country));
      marker.addTo(this.badgeLayer);
    }
  }
}
