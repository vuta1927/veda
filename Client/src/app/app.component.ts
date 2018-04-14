import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute, NavigationStart } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { merge } from 'rxjs/observable/merge'
import { filter, map, mergeMap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { SecurityService } from './shared/services/security.service';
import { ConfigurationService } from './shared/services/configuration.service';
import { Helpers } from './helpers';
import { I18nService } from './shared/services/i18n.service';
import { environment } from '../environments/environment';
import './shared/utils/string.extends';

@Component({
  selector: 'body',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

  title = 'app';
  globalBodyClass = 'm-page--loading-non-block m-page--fluid m--skin- m-content--skin-light2 m-header--fixed m-header--fixed-mobile m-aside-left--enabled m-aside-left--skin-dark m-aside-left--offcanvas m-footer--push m-aside--offcanvas-default';

  constructor(
    private router: Router,
    private translate: TranslateService,
    private securityService: SecurityService,
    private activatedRoute: ActivatedRoute,
    private titleService: Title,
    private configurationService: ConfigurationService,
    private i18nService: I18nService
  ) {
    // Setup translations
    this.translate.setDefaultLang('en-US');
    this.translate.use('en-US');

    const onNavigationEnd = router.events.pipe(filter((event) => event instanceof NavigationEnd));

    // Change page title on navigation or language change, based on route data
    merge(translate.onLangChange, onNavigationEnd)
      .pipe(
        map(() => {
          let route = this.activatedRoute;
          while (route.firstChild) {
            route = route.firstChild
          }
          return route;
        }),
        filter((route) => route.outlet === 'primary'),
        mergeMap((route) => route.data)
      )
      .subscribe((event) => {
        const title = event['title'];
        if (title) {
          this.titleService.setTitle(this.translate.instant(title));
        }
      });

    //Get configuration from server environment variables:
    this.configurationService.load();

    // this.configurationService.settingsLoaded$.subscribe(_ => securityService.Config());
    securityService.Config();
  }

  ngOnInit() {
    this.router.events.subscribe((route) => {
      if (route instanceof NavigationStart) {
        Helpers.setLoading(true);
        Helpers.bodyClass(this.globalBodyClass);
      }

      if (route instanceof NavigationEnd) {
        Helpers.setLoading(false);
      }
    })
  }
}
