import { ModuleWithProviders, NgModule, ErrorHandler } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule, HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';

// Extensions
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { OAuthModule } from 'angular-oauth2-oidc';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

// Services
import { NotificationService } from './services/notification.service';
import { StorageService } from './services/storage.service';
import { I18nService } from './services/i18n.service';
import { SpinnerService } from './services/spinner.service';
import { GlobalErrorHandler } from './services/global-error.service';
import { SecurityService } from './services/security.service';
import { UtilityService } from './services/utility.service';
import { FormService } from './services/form.service';
import { AuthGuard } from './guards/auth.guard';
import { ConfigurationService } from './services/configuration.service';
import { ScriptLoaderService } from './services/script-loader.service';

// Interceptors
import { SpinnerInterceptor } from './services/interceptors/spinner.interceptor';
import { AuthInterceptor } from './services/interceptors/auth.interceptor';

// Components

// Pipes

// Directives
import { UsersService } from '../+administrator/users/users.service';

import { LayoutModule } from './components/layouts/layout.module';
import { ThemeComponent } from './components/theme.component';
import { NgxFormErrorModule } from './utils/form-errors/ngx-errors.module';
import { TranslateHttpClientLoader } from './i18n/http-client-loader';
import { TranslateExtService } from './services/translate-ext.service';
import { RolesService } from '../+administrator/roles/roles.service';
import { ProjectService } from '../+project/project.service';
import { ImageService } from '../+project/services/image.service';
import { ClassService } from '../+project/services/class.service';
import { TagService } from '../+project/services/tag.service';
import { ProjectUserService } from '../+project/services/project-users.service';
export function httpTranslateLoader(http: HttpClient) {
    return new TranslateHttpClientLoader(http);
}

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule,
        TranslateModule,
        OAuthModule.forRoot(),
        NgbModule.forRoot(),
        HttpClientModule,
        NgxFormErrorModule.forRoot(),
        LayoutModule
    ],
    declarations: [
        ThemeComponent
    ],
    exports: [
        // Modules
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule,
        NgbModule,
        NgxFormErrorModule,
        TranslateModule,
        // Providers, Components, directive, pipes
        LayoutModule
    ]
})
export class SharedModule {
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: SharedModule,
            providers: [
                AuthGuard,
                NotificationService,
                StorageService,
                ConfigurationService,
                { provide: HTTP_INTERCEPTORS, useClass: SpinnerInterceptor, multi: true },
                { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
                // { provide: ErrorHandler, useClass: GlobalErrorHandler },
                I18nService,
                SpinnerService,
                SecurityService,
                UtilityService,
                FormService,
                UsersService,
                ScriptLoaderService,
                TranslateExtService,
                RolesService,
                ProjectService,
                ImageService,
                TagService,
                ClassService,
                ProjectUserService
            ]
        }
    }
}