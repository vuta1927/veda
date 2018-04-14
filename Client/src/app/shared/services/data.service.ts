import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

import { LocationStrategy } from '@angular/common';

@Injectable()
export class DataService {

  constructor(private http: HttpClient) {}

  public get<T>(url: string, params?: any): Observable<T> {
    return this.http.get<T>(url, { params: this.buildUrlSearchParams(params) });
  }

  public getFull<T>(url: string): Observable<HttpResponse<T>> {
    return this.http.get<T>(url, { observe: 'response' });
  }

  public post<T>(url: string, data?: any, params?: any): Observable<T> {
    return this.http.post<T>(url, data);
  }

  public put<T>(url: string, data?: any, params?: any): Observable<T> {
    return this.http.put<T>(url, data);
  }

  public delete<T>(url: string): Observable<T> {
    return this.http.delete<T>(url);
  }

  private buildUrlSearchParams(params: any): HttpParams {
    const searchParams = new HttpParams();
    for (const key in params) {
      if (params.hasOwnProperty(key)) {
        searchParams.append(key, params[key]);
      }
    }
    return searchParams;
  }
}
