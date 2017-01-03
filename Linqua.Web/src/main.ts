import './polyfills.ts';

import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { enableProdMode } from '@angular/core';
import { AppModule } from './app/';

// Enable either Hot Module Reloading or production mode
/* tslint:disable */
let hotModule = (<any>module)['hot'];
if (hotModule) {
    hotModule.accept();
    hotModule.dispose(() => { });
} else {
    enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule);
