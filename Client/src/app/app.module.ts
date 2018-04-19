import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { RouterModule, PreloadAllModules } from '@angular/router';
import { AdminGuard, UserGuard, RoleGuard, ProjectGuard } from '../app/shared/guards/auth.guard';
import { DataService } from '../app/shared/services/data.service';
import { PermissionService } from '../app/+administrator/roles/permission/permission.service';
/*
 * Platform and Environment providers/directives/pipes
 */
import { ROUTES } from './app.routes';
// App is our top level component
import { AppComponent } from './app.component';

import { SharedModule } from './shared/shared.module';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpClientLoader } from './shared/i18n/http-client-loader';

import { ToastModule } from 'ng2-toastr/ng2-toastr';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import { ColorPickerModule } from 'ngx-color-picker'
@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    FormsModule,
    
    ToastModule.forRoot(),
    BrowserAnimationsModule,
    ColorPickerModule,
    
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: (http: HttpClient) => new TranslateHttpClientLoader(http),
        deps: [HttpClient]
      }
    }),

    SharedModule.forRoot(),

    RouterModule.forRoot(ROUTES, {
      useHash: Boolean(history.pushState) === false,
      preloadingStrategy: PreloadAllModules
    })
  ],
  providers: [AdminGuard, UserGuard, RoleGuard, ProjectGuard, DataService, PermissionService],
  bootstrap: [AppComponent]
})
export class AppModule { }
