import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { SpinnerService } from '../spinner.service';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/finally';

@Injectable()
export class SpinnerInterceptor implements HttpInterceptor {
  private requestCounter: number = 0;

  constructor(private spinnerService: SpinnerService) { }

  public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    this.spinnerService.start();
    this.requestCounter++;
    return next.handle(req).finally(() => {
      this.requestCounter--;
      if (this.requestCounter === 0) {
        this.spinnerService.stop();
      }
    });
  }
}
