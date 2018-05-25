import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { UserProfileComponent } from './userProfile.component';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { routes } from './userProfile.routing';
@NgModule({
    imports: [SharedModule,RouterModule.forChild(routes), DxDataGridModule, DxMenuModule],
    declarations: [UserProfileComponent],
    entryComponents: []
})
export class UserProfileModule {}