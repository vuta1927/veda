import { Injectable } from '@angular/core';
import { LangChangeEvent, TranslateService } from '@ngx-translate/core';

const languageKey = 'language';

import * as enUS from '../../../assets/i18n/en-US.json';
import * as viVN from '../../../assets/i18n/vi-VN.json';

@Injectable()
export class I18nService {
  public supportedLanguages: any[];

  public defaultLanguage: any;

  constructor(private translateService: TranslateService) {
    // Embed languages to avoid extra HTTP requests
    translateService.setTranslation('en-US', enUS);
    translateService.setTranslation('vi-VN', viVN);
  }

  /**
   * Initializes i18n for the application.
   * Loads language from local storage if present, or sets default language.
   * @param {!string} defaultLanguage The default language to use.
   * @param {Array.<String>} supportedLanguages The list of supported languages.
   */
  public init(defaultLanguage: any, supportedLanguages: any[]) {
    this.defaultLanguage = defaultLanguage;
    this.supportedLanguages = supportedLanguages;
    this.language = '';

    // this.translateService.setDefaultLang(defaultLanguage.key);

    this.translateService
      .onLangChange
      .subscribe((event: LangChangeEvent) => {
        localStorage.setItem(languageKey, event.lang);
      });
  }

  /**
   * Sets the current language.
   * Note: The current language is saved to the local storage.
   * If no parameter is specified, the language is loaded from local storage (if present).
   * @param {Object} language The IETF language code to set.
   */
  set language(language: any) {
    let languageCode = language.key || localStorage.getItem(languageKey) ||
      this.translateService.getBrowserCultureLang();
    let isSupportedLanguage = this.supportedLanguages.some(function(el) {
      return el.key === languageCode;
    });

    // If no exact match is found, search without the region
    if (language && !isSupportedLanguage) {
      languageCode = language.key.split('-')[0];
      languageCode = this.supportedLanguages
        .find((supportedLanguage) => supportedLanguage.key.startsWith(languageCode)) || '';
      if (languageCode) {
        languageCode = languageCode.key;
      }
      isSupportedLanguage = Boolean(languageCode);
    }

    // Fallback if language is not supported
    if (!isSupportedLanguage) {
      languageCode = this.defaultLanguage.key;
    }
console.log(languageCode)
    this.translateService.use(languageCode);
  }

  /**
   * Gets the current language.
   * @return {string} The current language code.
   */
  get language(): any {
    return this.supportedLanguages.find((sp) => sp.key === this.translateService.currentLang);
  }
}
