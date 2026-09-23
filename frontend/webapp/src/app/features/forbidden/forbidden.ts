import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Icon } from '../../shared/components/icon/icon';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [RouterLink, Icon],
  templateUrl: './forbidden.html',
  styleUrl: './forbidden.scss'
})
export class Forbidden {}
