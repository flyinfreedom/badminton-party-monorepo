import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import liff from '@line/liff';
import { BehaviorSubject } from 'rxjs';
import { DialogService } from './dialog.service';

interface ILineUserProfile {
  userId: string;
  displayName: string;
  pictureUrl: string | undefined;
}

@Injectable({
  providedIn: 'root'
})
export class LiffService {
  lineUserProfile: ILineUserProfile | null = null;
  gettedProfile$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  constructor(private dialogService: DialogService) { }

  initLIFF(): void {
    liff.init({
      liffId: environment.liffid
    }).then(async () => {
      if (!liff.isLoggedIn()) {
        liff.login();
      } else {
        const profile = await liff.getProfile();
        this.lineUserProfile = {
          userId: profile.userId,
          displayName: profile.displayName,
          pictureUrl: profile.pictureUrl
        };
        this.gettedProfile$.next(true);
      }
    })
  }

  getLiffUrl(): string {
    return `https://liff.line.me/${environment.liffid}`
  }

  getAccessToken(): string | null {
    return liff.getAccessToken();
  }

  logout(): void {
    liff.logout();
    location.reload();
  }

  shareGroup(buttonMessage: any, altText: string): void {
    if (liff.isApiAvailable('shareTargetPicker')) {
      liff.shareTargetPicker([
        {
          type: 'template',
          altText: altText,
          template: buttonMessage
        }
      ]).then((res) => {
          if(!!res) {
            this.dialogService.openSuccessDialog('分享成功');
          }
        })
        .catch((error) => {
          this.dialogService.openFailureDialog('分享失敗', error);
        });
    }
  }
}
