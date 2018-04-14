// support NodeJS modules without type definitions
declare module '*';

/* SystemJS module definition */
declare var module: NodeModule;
interface NodeModule {
  id: string;
}
interface String {
	format(...replacements: string[]): string;
	toFomattedString(useLocale, args): string;
}
interface JQuery {
	mMenu(options: any): JQuery;
	animateClass(options: any): JQuery;
	setActiveItem(item: any): JQuery;
	getPageTitle(item: any): JQuery;
	getBreadcrumbs(item: any): JQuery;
	validate(options: any): JQuery;
	valid(): JQuery;
	resetForm(): JQuery;
	markdown(): JQuery;
}

declare var App: { 
	init(): void;
	initAjax(): void;
	scrollTo(element, top): void;
	isRTL(): boolean;
};

declare var Demo: {
	init(): void;
};

declare var Layout: {
	init(): void;
	initAjax(): void;
	scrollTo(element, top): void;
};

declare var QuickSidebar: {
	init(): void;
};