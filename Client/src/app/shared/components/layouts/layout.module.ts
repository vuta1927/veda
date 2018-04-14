import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { AsideLeftMinimizeDefaultEnabledComponent } from '../pages/aside-left-minimize-default-enabled/aside-left-minimize-default-enabled.component';
import { HeaderNavComponent } from './header-nav/header-nav.component';
import { DefaultComponent } from '../pages/default/default.component';
import { AsideNavComponent } from './aside-nav/aside-nav.component';
import { FooterComponent } from './footer/footer.component';
import { QuickSidebarComponent } from './quick-sidebar/quick-sidebar.component';
import { ScrollTopComponent } from './scroll-top/scroll-top.component';
import { UnwrapTagDirective } from '../../directives/unwrap-tag.directive';
import { HrefPreventDefaultDirective } from '../../directives/href-prevent-default.directive';
import { BreadcrumbsComponent } from './navigation/breadcrumbs.component';
import { QuickActionComponent } from './quick-action/quick-action.component';
import { FormAlertComponent } from './form-alert/form-alert.component';

@NgModule({
	declarations: [
		LayoutComponent,
		AsideLeftMinimizeDefaultEnabledComponent,
		HeaderNavComponent,
		DefaultComponent,
		AsideNavComponent,
		FooterComponent,
		QuickSidebarComponent,
		ScrollTopComponent,
		HrefPreventDefaultDirective,
		UnwrapTagDirective,
		BreadcrumbsComponent,
		QuickActionComponent,
		FormAlertComponent
	],
	exports: [
		LayoutComponent,
		AsideLeftMinimizeDefaultEnabledComponent,
		HeaderNavComponent,
		DefaultComponent,
		AsideNavComponent,
		FooterComponent,
		QuickSidebarComponent,
		ScrollTopComponent,
		UnwrapTagDirective,
		HrefPreventDefaultDirective,
		BreadcrumbsComponent,
		QuickActionComponent,
		FormAlertComponent
	],
	imports: [
		CommonModule,
		RouterModule,
	]
})
export class LayoutModule {
}