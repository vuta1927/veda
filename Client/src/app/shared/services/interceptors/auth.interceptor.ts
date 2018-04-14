import { Injectable, Injector } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';

import { Observable } from 'rxjs/Rx';
import { SecurityService } from '../security.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private securityService: SecurityService) { }

  public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Get the auth header from the server
    // const auth: SecurityService = this.injector.get(SecurityService);

    if (!this.securityService.IsAuthorized) {
      return next.handle(req);
    }

    const authHeader = this.securityService.getToken();
    // Clone the request to add the new header

    // const authReq = req.clone({ headers: req.headers.set('Authorization', authHeader) });
    // OR shortcut
    const authReq = req.clone({ setHeaders: { Authorization: 'Bearer ' + authHeader } });
    // Pass on the cloned request instead of the original request.
    return next.handle(authReq);
  }
}
