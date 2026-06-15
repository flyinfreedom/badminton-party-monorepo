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
    if (req.url.trim().startsWith('api/member/avatar')) {
      req = req.clone({
        setHeaders: {
          Authorization: this.liffService.getAccessToken() ?? '',
        },
      });
    } else {
      req = req.clone({
        setHeaders: {
          'Content-Type': 'application/json; charset=utf-8',
          Accept: 'application/json',
          Authorization: this.liffService.getAccessToken() ?? '',
        },
      });
    }

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.liffService.logout();
        }

        if (error.status === 701) {
          this.liffService.gettedProfile$.next(true);
          this.dialogService.openFailureDialog('Token 失效，已重新取得，請重新再試');
        }
        throw error;
      })
    );
  }
}
