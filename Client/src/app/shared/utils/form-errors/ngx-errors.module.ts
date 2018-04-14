import { NgModule, ModuleWithProviders } from "@angular/core";
import { NgxErrorsComponent } from "./ngx-errors.component";
import { NgxErrorTemplateDirective } from "./ngx-errors-template.directive";
import { CommonModule } from "@angular/common";
import { NgxErrorsService } from "./ngx-errors.service";
import { NgxErrorsContentDirective } from "./ngx-errors-content.directive";

@NgModule({
    declarations: [
        NgxErrorsComponent,
        NgxErrorTemplateDirective,
        NgxErrorsContentDirective
    ],
    imports: [CommonModule],
    exports: [
        NgxErrorsComponent,
        NgxErrorTemplateDirective,
        NgxErrorsContentDirective
    ]
})
export class NgxFormErrorModule {
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: NgxFormErrorModule,
            providers: [NgxErrorsService]
        }
    }
}