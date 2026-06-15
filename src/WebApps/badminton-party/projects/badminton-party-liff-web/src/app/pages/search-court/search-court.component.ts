import { Component, OnInit, inject } from '@angular/core';
import { ICourt } from '../../models';
import { ApiService } from '../../services/api.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-search-court',
  templateUrl: './search-court.component.html',
  styleUrls: ['./search-court.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class SearchCourtComponent implements OnInit {
  courts: ICourt[] | null = null;
  private apiService = inject(ApiService);

  constructor() { }

  ngOnInit(): void {
    this.apiService.getCourts().subscribe(res => {
      this.courts = res;
    });
  }
}
