import { Component, inject } from '@angular/core';
import { ProfileService } from './../../services/profile.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from '../header/header.component';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, HeaderComponent]
})
export class LayoutComponent {
  inited: boolean = false;

  constructor() {
    const profileService = inject(ProfileService);
    profileService.inited$.subscribe(result => this.inited = result);
  }
}
