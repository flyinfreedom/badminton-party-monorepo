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
          if (!res || !res.profile || !res.token) {
            throw Error('get profile failed');
          }
          this.liffService.setSysToken(res.token);
          this.profile = res.profile;
          this.pictureUrl = res.profile.pictureUrl;
          this.inited$.next(true);
        });
      }
    })
  }

  handleRecentOpening(courtId: string | null, courtName: string, location: string): void {
    let finalCourtId = courtId;
    
    if (!finalCourtId) {
      const existing = this.profile!.recentOpenings.find(c => c.courtName === courtName && c.location === location);
      finalCourtId = existing ? existing.courtId : `${this.profile?.memberId}_${new Date().getTime()}`;
    }

    const court = this.profile!.recentOpenings.find(court => court.courtId === finalCourtId);

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
      courtId: finalCourtId,
      courtName: courtName,
      location: location,
      openingTime: this.datePipe.transform(new Date(), 'yyyy-MM-ddTHH:mm:ss')!
    });
  }
}
