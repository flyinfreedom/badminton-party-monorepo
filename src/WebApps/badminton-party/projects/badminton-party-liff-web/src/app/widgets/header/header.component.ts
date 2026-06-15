import { Component, inject } from '@angular/core';
import { ProfileService } from './../../services/profile.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule
  ]
})
export class HeaderComponent {
  private profileService = inject(ProfileService);

  constructor() {}

  getAvatarUrl(): string {
    if(!!this.profileService.pictureUrl) {
      return this.profileService.pictureUrl;
    }
    return '';
  }
}
