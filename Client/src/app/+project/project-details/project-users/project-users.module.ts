import { NgModule } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './project-users.routing';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { ProjectUsersComponent } from './project-users.component';
import { CreateUpdateProjectUserComponent } from './create-update-project-user/create-update-project-user.component';
// import { DataService } from './data.service';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule],
    declarations: [ProjectUsersComponent, CreateUpdateProjectUserComponent],
    entryComponents : [CreateUpdateProjectUserComponent],
    providers: []
})

export class ProjectUsersModule {}