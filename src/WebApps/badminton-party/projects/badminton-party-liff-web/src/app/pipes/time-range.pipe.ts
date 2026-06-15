import { Pipe, PipeTransform } from '@angular/core';
import { StringExtensions } from '../utils/string.extension';

@Pipe({
  name: 'timeRange',
  standalone: true
})
export class TimeRangePipe implements PipeTransform {
  constructor() {}

  transform(startTimeStr: string, playTime: number, showHours: boolean): string {
    const startTime = new Date(startTimeStr.replace('Z', ''));showHours
    const endTime = new Date(new Date(startTimeStr).setHours(startTime.getHours() + playTime));
    return `${StringExtensions.padLeft(startTime.getHours(), 2)}:00 ~ ${StringExtensions.padLeft(endTime.getHours(), 2)}:00` + (showHours ? ` (共 ${playTime} 小時)` : '');
  }

}
