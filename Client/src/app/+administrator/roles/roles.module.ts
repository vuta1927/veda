import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './roles.routing';
import { RolesComponent } from './roles.component';

import { DxDataGridModule, DxMenuModule,DxCheckBoxModule, DxSelectBoxModule } from 'devextreme-angular';
import { CreateOrUpdateRoleComponent } from './create-or-update/create-or-update-role.component';
import { AddPermissionToRoleComponent } from './permission/add-permission-to-role.component';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule,DxCheckBoxModule, DxSelectBoxModule],
    declarations: [RolesComponent, CreateOrUpdateRoleComponent, AddPermissionToRoleComponent],
    entryComponents: [CreateOrUpdateRoleComponent, AddPermissionToRoleComponent]
})
export class RolesModule {}