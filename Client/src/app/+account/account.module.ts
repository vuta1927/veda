import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { AccountComponent } from './account.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { routes } from './account.routing';
import { LoginComponent } from './login/login.component';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { LogoutComponent } from './logout/logout.component';

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routes),
        TranslateModule
    ],
    declarations: [
        AccountComponent,
        LoginComponent,
        LogoutComponent
    ]
})
export class AccountModule {

}