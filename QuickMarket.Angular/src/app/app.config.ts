import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withInMemoryScrolling, withRouterConfig } from '@angular/router';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(
      routes,

      // Scroll a anclas y restauración
      withInMemoryScrolling({
        // 'top' sube al inicio cuando cambias de ruta sin ancla;
        // usa 'enabled' si quieres restaurar la posición anterior.
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled',
      }),

      // Offset para que el header sticky no tape el ancla
      withRouterConfig({

      }),
    ),

    provideClientHydration(withEventReplay()),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideBrowserGlobalErrorListeners(),
  ],
};
