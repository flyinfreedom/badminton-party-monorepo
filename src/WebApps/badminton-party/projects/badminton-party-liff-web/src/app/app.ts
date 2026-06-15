import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LiffService } from './services/liff.service';
import { HubService } from './services/hub.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly liffService = inject(LiffService);
  private readonly hubService = inject(HubService);

  constructor() {
    this.hubService.startConnection();
  }

  ngOnInit(): void {
    this.liffService.initLIFF();
  }
}
