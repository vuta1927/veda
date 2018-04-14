import { Injectable, SkipSelf, Optional } from "@angular/core";
import { ControlContainer } from "@angular/forms";
import { NgxDisplayContextStore, NgxDisplayContext } from "./ngx-errors.model";
import { NgxErrorsComponent } from "./ngx-errors.component";
import { Provider } from "@angular/compiler/src/core";
import { TranslateExtService } from "../../services/translate-ext.service";

export type ContextScope = ControlContainer | NgxErrorsComponent;

export interface IErrorMessage {
    message: string;
    params?: (args) => string[];
}

@Injectable()
export class NgxErrorsService {
    defaultContext = new NgxDisplayContextStore();

    protected defaultMsgs: { [name: string]: IErrorMessage } = {
        'required': { message: 'This field is required.' },
        'email': { message: 'Please enter a valid email address.' },
        'minlength': { message: 'The min number of characters is {0}.', params: (args) => { return [args.requiredLength] } },
        'maxlength': { message: 'The max allowed number of characters is {0}.', params: (args) => { return [args.requiredLength] } },
        'pattern': { message: 'The required pattern is: {0}.', params: (args) => { return [args.requiredPattern] } }
    };

    constructor(private translateExtService: TranslateExtService) { }

    protected scopes = new Map<ContextScope, NgxDisplayContextStore>();

    getContextStore(scope: ContextScope, alt?: ContextScope): NgxDisplayContextStore {
        return (scope && this.scopes.get(scope)) || (alt && this.scopes.get(alt)) || this.defaultContext;
    }

    getContext(scope: ContextScope, alt?: ContextScope): NgxDisplayContext;
    getContext(errorKey: string, alt?: ContextScope): NgxDisplayContext;
    getContext(errorKey: string, scope: ContextScope, alt?: ContextScope): NgxDisplayContext;
    getContext(errorKey?: string | ContextScope,
        scope?: ContextScope,
        alt?: ContextScope): NgxDisplayContext {
        if (errorKey && typeof errorKey !== 'string') {
            if (scope) {
                alt = <any>scope;
            }
            scope = errorKey;
        }

        const store = this.getContextStore(scope, alt) || this.defaultContext;
        return store.get(errorKey as string);
    }

    setContext(context: NgxDisplayContext, scope?: ContextScope, parent?: ContextScope): void {
        if (scope) {
            let store = this.scopes.get(scope);
            if (!store) {
                const parentStore = (parent && this.getContextStore(parent)) || this.defaultContext;
                this.scopes.set(scope, store = new NgxDisplayContextStore(parentStore));
            }
            store.add(context);
        } else {
            this.defaultContext.add(context);
        }
    }

    removeScope(scope: ContextScope): boolean {
        return this.scopes.delete(scope);
    }

    removeContext(context: NgxDisplayContext, scope?: ContextScope): boolean {
        let result: boolean = false;

        if (scope) {
            const store = this.scopes.get(scope);
            if (store) {
                result = store.remove(context.errorKey);
            }
            if (!store.hasAny()) {
                this.scopes.delete(scope);
            }
        } else {
            result = this.defaultContext.remove(context.errorKey);
        }

        return result;
    }

    setDefaultMessage(name: string, errors: IErrorMessage): void {
        this.defaultMsgs[name] = errors;
    }

    getDefaultMessage(name: string, params?: any): string | undefined {
        if (!this.defaultMsgs[name])
        {
            console.log(name);
            return name;
        }
        let translated: string = this.translateExtService.get(this.defaultMsgs[name].message);
        return this.defaultMsgs[name].params
            ? ''.format.apply(translated, this.defaultMsgs[name].params(params))
            : translated;
    }

    static clone(base: NgxErrorsService, translateService: TranslateExtService): NgxErrorsService {
        const ngxErrorsService = new NgxErrorsService(translateService);
        if (base) {
            Object.assign(ngxErrorsService.defaultMsgs, base.defaultMsgs);
            ngxErrorsService.defaultContext = base.defaultContext.clone(base.defaultContext);
            Array.from(base.scopes.entries())
                .forEach(([k, v]) => ngxErrorsService.scopes.set(k, v.clone(base.defaultContext)));
        }
        return ngxErrorsService;
    }
}

@Injectable()
export class NgxErrorsServiceContainer {
    constructor(@SkipSelf() @Optional() public ngxErrorsService: NgxErrorsService, public translateService: TranslateExtService) { }
}

export function containerFactory(c: NgxErrorsServiceContainer) {
    return NgxErrorsService.clone(c.ngxErrorsService, c.translateService);
}

export const NGX_ERRORS_SERVICE_CHILD_PROVIDERS: Provider[] = [
    NgxErrorsServiceContainer,
    {
        provide: NgxErrorsService,
        useFactory: containerFactory,
        deps: [NgxErrorsServiceContainer, TranslateExtService]
    }
];