import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';

@Component({
  selector: 'app-progress-bar',
  templateUrl: './progress-bar.component.html',
  styleUrls: ['./progress-bar.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class ProgressBarComponent implements OnInit {
  @Input('max')
  maxNumber: number = 1;

  @Input('alternate')
  alternateNumber: number = 1;

  @Input('current')
  currentNumber: number = 0;

  constructor(public sanitizer: DomSanitizer) {

  }

  ngOnInit(): void {
  }

  getAlternatePercentage(): number {
    return Math.min((this.currentNumber / (this.maxNumber + this.alternateNumber)) * 100, 100)
  }


  getCurrentPercentage(): number {
    return Math.min((Math.min(this.currentNumber / this.maxNumber, 1) / ((this.maxNumber + this.alternateNumber) / this.maxNumber)) * 100, 100)
  }

  getMaxPercentage(): SafeStyle {
    return this.sanitizer.bypassSecurityTrustStyle(`calc(${(this.maxNumber / (this.maxNumber + this.alternateNumber) * 100)}% - 14px)`);
  }
}
