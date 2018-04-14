import { TranslateLoader } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';

export class TranslateHttpClientLoader implements TranslateLoader {
    constructor(
        private httpClient: HttpClient,
        public prefix: string = './assets/i18n/',
        public suffix: string = '.json') {}

    public getTranslation(lang: string): any {
        console.log(`${this.prefix}${lang}${this.suffix}`);
        return this.httpClient.get(`${this.prefix}${lang}${this.suffix}`);
    }
}
