import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './merge-project.routing';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { MergeProject } from './merge-project.component';
import { ColorPickerModule } from 'ngx-color-picker';
import { MergeClass } from './merge-class/merge-class.component';
import { UpdateUserComponent } from './update-user/update-user.component';
import { DataService } from '../data.service';
// import { DataService } from './data.service';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule, ColorPickerModule],
    declarations: [MergeProject, MergeClass, UpdateUserComponent],
    entryComponents : [MergeClass, UpdateUserComponent],
    providers: [DataService]
})

export class MergeProjectModule {}