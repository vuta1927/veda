import { NgModule } from '@angular/core';
import { SharedModule } from '../../../../shared/shared.module';
import { RouterModule } from '@angular/router';
import { routes } from './export-project.routing';
import { DxDataGridModule, DxMenuModule } from 'devextreme-angular';
import { ExportProjectComponent } from './export-project.component';
// import { DataService } from './data.service';
@NgModule({
    imports: [SharedModule, RouterModule.forChild(routes), DxDataGridModule, DxMenuModule],
    declarations: [ExportProjectComponent,],
    entryComponents : [],
    providers: []
})

export class ExportProjectModule {}