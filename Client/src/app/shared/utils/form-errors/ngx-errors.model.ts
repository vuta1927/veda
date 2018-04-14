import { TemplateRef } from "@angular/core";

export interface ErrorItem {
    name: string;
    message: string;
}

export interface NgxErrorsContext {
    $implicit: ErrorItem;
}

export interface NgxDisplayContext {
    template: TemplateRef<NgxErrorsContext>;

    maxError?: number;

    errorKey?: string | undefined;
}

export class NgxDisplayContextStore {
    get global(): NgxDisplayContext { return this._global; }
    get hash(): any {
        this.syncHash();
        return this._hash;
    }

    private _hash: any = 0;
    private _parentHash: any;
    private _mergeKeys: { [key: string]: NgxDisplayContext };
    private _global: NgxDisplayContext;
    private keyed = new Map<string, NgxDisplayContext>();

    constructor(private parent?: NgxDisplayContextStore) { }

    add(ctx: NgxDisplayContext): void {
        if (ctx.errorKey) {
            this.reHash();
            this.keyed.set(ctx.errorKey, ctx);
        } else {
            this._global = ctx;
        }
    }

    hasAny(): boolean {
        return this.hasGlobal() || this.keyed.size > 0 || (this.parent && this.parent.hasAny());
    }

    hasGlobal(): boolean {
        return !!this._global || (this.parent && this.parent.hasGlobal());
    }

    has(key: string): boolean {
        return this.keyed.has(key) || (this.parent && this.parent.has(key));
    }

    get(): NgxDisplayContext | undefined;
    get(key: string, defaultToGlobal: false): NgxDisplayContext | undefined;
    get(key: string): NgxDisplayContext | undefined;
    get(key?: string, defaultToGlobal?: boolean): NgxDisplayContext | undefined {
        if (!key) {
            return this._global || (this.parent && this.parent.get());
        }
        return this.keyed.get(key)
            || (this.parent && this.parent.get(key, false))
            || (defaultToGlobal !== false && this.get());
    }

    mergedKeys(): { [key: string]: NgxDisplayContext } {
        this.syncHash();
        if (!this._mergeKeys) {
            const keyedAsObject = Array.from(this.keyed.entries()).reduce((obj, [k, v]) => {
                obj[k] = v;
                return obj;
            }, {});

            if (this.parent) {
                this._mergeKeys = Object.assign(this.parent.mergedKeys(), keyedAsObject);
            } else {
                this._mergeKeys = keyedAsObject;
            }
        }
        return this._mergeKeys;
    }

    remove(key?: string): boolean {
        if (key && this.keyed.has(key)) {
            this.reHash();
            return this.keyed.delete(key);
        } else if (this._global) {
            this._global = undefined;
            return true;
        }
        return false;
    }

    clone(parent?: NgxDisplayContextStore): NgxDisplayContextStore {
        const store = new NgxDisplayContextStore(parent);
        store._global = this._global;
        store.keyed = new Map<string, NgxDisplayContext>(this.keyed.entries());
        return store;
    }

    private reHash(): void {
        this._mergeKeys = undefined;
        this._hash += 1;
    }

    private syncHash(): void {
        if (this.parent) {
            const pHash = this.parent.hash;
            if (pHash !== this._parentHash) {
                this._parentHash = pHash;
                this.reHash();
            }
        }
    }
}