import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError } from 'rxjs';
import { LiffService } from '../services/liff.service';
import { DialogService } from '../services/dialog.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private liffService: LiffService, private dialogService: DialogService) { }

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const isInitReq = req.url.trim().includes('api/member/init');
    const isAvatarReq = req.url.trim().startsWith('api/member/avatar');

    let authHeaderValue = '';
    if (isInitReq) {
      authHeaderValue = this.liffService.getAccessToken() ?? '';
    } else {
      const sysToken = this.liffService.getSysToken();
      authHeaderValue = sysToken ? `Bearer ${sysToken}` : (this.liffService.getAccessToken() ?? '');
    }

    if (isAvatarReq) {
      req = req.clone({
        setHeaders: {
          Authorization: authHeaderValue,
        },
      });
    } else {
      req = req.clone({
        setHeaders: {
          'Content-Type': 'application/json; charset=utf-8',
          Accept: 'application/json',
          Authorization: authHeaderValue,
        },
      });
    }

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          if (isInitReq) {
            this.liffService.logout();
          } else {
            this.liffService.clearSysToken();
            this.liffService.gettedProfile$.next(true);
            this.dialogService.openFailureDialog('登入逾期，已重新取得授權，請重試剛才的操作');
          }
        }
        throw error;
      })
    );
  }
}
