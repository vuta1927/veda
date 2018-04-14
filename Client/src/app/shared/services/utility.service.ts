import { Injectable } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpResponseBase } from '@angular/common/http';

@Injectable()
export class UtilityService {
    constructor(private router: Router, private route: ActivatedRoute) { }

    public navigateToReturnUrl() {
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigate([returnUrl]);
    }

    public getParams() {
        const searchParams = window.location.search.split('?')[1];
        if (searchParams) {
            const paramsObj: any = {};

            searchParams.split('&').forEach((i) => {
                paramsObj[i.split('=')[0]] = i.split('=')[1];
            });

            return paramsObj;
        }

        return undefined;
    }

    public toQueryParams(obj: any): string {
        return Object.keys(obj)
            .map((key) => encodeURIComponent(key) + '=' + encodeURIComponent(obj[key]))
            .join('&');
    }

    public checkNoNetwork(response: HttpResponseBase) {
        if (response instanceof HttpResponseBase) {
            return response.status === 0;
        }

        return false;
    }

    public getQueryParamsFromString(paramString: string) {

        if (!paramString) {
            return null;
        }

        const params: { [key: string]: string } = {};

        for (const param of paramString.split('&')) {
            const keyValue = this.splitInTwo(param, '=');
            params[keyValue.firstPart] = keyValue.secondPart;
        }

        return params;
    }

    public splitInTwo(text: string, separator: string): { firstPart: string, secondPart: string } {
        const separatorIndex = text.indexOf(separator);

        if (separatorIndex === -1) {
            return { firstPart: text, secondPart: null };
        }

        const part1 = text.substr(0, separatorIndex).trim();
        const part2 = text.substr(separatorIndex + 1).trim();

        return { firstPart: part1, secondPart: part2 };
    }

    public removeNulls(obj) {
        const isArray = obj instanceof Array;

        for (const k in obj) {
            if (obj.hasOwnProperty(k)) {
                if (obj[k] === null) {
                    isArray ? obj.splice(k, 1) : delete obj[k];
                } else if (typeof obj[k] === 'object') {
                    this.removeNulls(obj[k]);
                }

                if (isArray && obj.length === k) {
                    this.removeNulls(obj);
                }
            }
        }

        return obj;
    }

    public stripTrailingSlash(str: string): string {
        return str.replace(/\/$/, "");
    }

    public getRandomInt(start, end) {
        return Math.floor(Math.random() * end) + start;
      }
      public getRandomColor() {
        return '#' + (this.pad(this.getRandomInt(0, 255).toString(16), 2) + this.pad(this.getRandomInt(0, 255).toString(16), 2) + this.pad(this.getRandomInt(0, 255).toString(16), 2));
      }
      public pad(str, length) {
        while (str.length < length) {
          str = '0' + str;
        }
        return str;
      }
      public getRandomNum(min, max) {
        return Math.random() * (max - min) + min;
      }
}