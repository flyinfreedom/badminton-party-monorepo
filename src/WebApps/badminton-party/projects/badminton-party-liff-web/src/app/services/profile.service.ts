import { Injectable } from '@angular/core';
import { LiffService } from './liff.service';
import { ApiService } from './api.service';
import { IMemberProfile } from '../models/api-model';
import { BehaviorSubject } from 'rxjs';
import { DatePipe } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  pictureUrl: string | undefined = undefined;
  profile: IMemberProfile | null = null;
  inited$: BehaviorSubject<boolean> = new BehaviorSubject(false);

  constructor(
    private liffService: LiffService,
    public datePipe: DatePipe,
    private apiService: ApiService) {
    liffService.gettedProfile$.subscribe(inited => {
      if (inited) {
        apiService.getMemberProfile({
          lineUserId: liffService.lineUserProfile!.userId,
          memberName: liffService.lineUserProfile!.displayName,
          pictureUrl: liffService.lineUserProfile!.pictureUrl + ''
        }).subscribe(res => {
          if (!res) {
            throw Error('get profile failed');
          }
          this.profile = res;
          this.pictureUrl = res.pictureUrl;
          this.inited$.next(true);
        });
      }
    })
  }

  handleRecentOpening(courtId: string, courtName: string, location: string): void {
    const court = this.profile!.recentOpenings.find(court => court.courtId === courtId);

    if(!!court) {
      court.openingTime = this.datePipe.transform(new Date(), 'yyyy-MM-ddTHH:mm:ss')!;
      this.profile!.recentOpenings = this.profile!.recentOpenings.sort((a, b) => {
        return new Date(b.openingTime).getTime() - new Date(a.openingTime).getTime();
      });
      return;
    }

    this.profile!.recentOpenings.sort((a, b) => {
      return new Date(b.openingTime).getTime() - new Date(a.openingTime).getTime();
    }).splice(2, this.profile!.recentOpenings.length);

    this.profile!.recentOpenings.push({
      courtId: courtId,
      courtName: courtName,
      location: location,
      openingTime: this.datePipe.transform(new Date(), 'yyyy-MM-ddTHH:mm:ss')!
    });
  }
}
