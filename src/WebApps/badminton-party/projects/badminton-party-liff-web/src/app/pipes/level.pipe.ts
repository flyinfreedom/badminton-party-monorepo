import { Pipe, PipeTransform } from '@angular/core';
import { Level, LevelGroup } from '../enums';

@Pipe({
  name: 'level',
  standalone: true
})
export class LevelPipe implements PipeTransform {
  mapper: Map<LevelGroup, string> = new Map<LevelGroup, string>();

  constructor() {
    this.mapper.set(LevelGroup.NotLimited, '不限');
    this.mapper.set(LevelGroup.Newcomer, '新手');
    this.mapper.set(LevelGroup.Junior, '初級');
    this.mapper.set(LevelGroup.JuniorToIntermediate, '初中級');
    this.mapper.set(LevelGroup.Intermediate, '中級');
    this.mapper.set(LevelGroup.IntermediateToSenior, '中高級');
    this.mapper.set(LevelGroup.Senior, '高級');
  }

  transform(value: LevelGroup, ...args: unknown[]): string {
    return this.mapper.get(value)!;
  }
}
