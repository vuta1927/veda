import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './users.routing';
import { UsersComponent } from './users.component';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { CreateOrUpdateUserComponent } from './create-or-update-user.component';

@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule],
    declarations: [UsersComponent, CreateOrUpdateUserComponent],
    entryComponents: [CreateOrUpdateUserComponent]
})
export class UsersModule {}