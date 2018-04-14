import { Injectable } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";

@Injectable()
export class TranslateExtService {

    constructor(private service: TranslateService) {}

    public get(text: string): string {
        let translatedText = text;
        this.service.get(text).subscribe((res: string) => {
            translatedText = res;
        });
        return translatedText;
    }
}