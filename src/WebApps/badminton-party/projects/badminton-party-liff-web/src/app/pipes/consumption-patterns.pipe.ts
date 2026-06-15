import { Pipe, PipeTransform } from '@angular/core';
import { ConsumptionPatterns } from '../enums';

@Pipe({
  name: 'consumptionPatterns',
  standalone: true
})
export class ConsumptionPatternsPipe implements PipeTransform {
  map: Map<ConsumptionPatterns, string> = new Map<ConsumptionPatterns, string>();

  constructor() {
    this.map.set(ConsumptionPatterns.Free, "免費");
    this.map.set(ConsumptionPatterns.AA, "平分");
  }

  transform(consumptionPatterns: ConsumptionPatterns, amount: number = 0): string {
    if (consumptionPatterns === ConsumptionPatterns.Fixed) {
      return amount.toString();
    }
    return this.map.get(consumptionPatterns)!;
  }
}
