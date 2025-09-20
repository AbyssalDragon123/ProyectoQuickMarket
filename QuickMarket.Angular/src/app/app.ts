import { Component, signal, inject, PLATFORM_ID, AfterViewInit, OnDestroy } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { RouterOutlet } from '@angular/router';
import { Header } from './components/header/header';
import { Footer } from './components/footer/footer';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Header, Footer],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements AfterViewInit, OnDestroy {
  protected readonly title = signal('WebSiteProject');

  // Modo políticas (para tu lógica de footer, etc.)
  isPolicyRoute = false;

  private platformId = inject(PLATFORM_ID);
  // guardamos el handler para poder removerlo en ngOnDestroy
  private onResize = () => this.setHeaderVar();

  constructor(private router: Router) {
    this.router.events.subscribe((e) => {
      if (e instanceof NavigationEnd) {
        const url = e.urlAfterRedirects || e.url;
        this.isPolicyRoute = /\/(terminos-condiciones|politica-privacidad|politica-devoluciones)(\/|$|\?|#)/.test(url);
        // por si el header cambia de alto en alguna ruta, refrescamos el offset
        this.setHeaderVar();
      }
    });
  }

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.setHeaderVar();                          // fija --header-h al cargar
    window.addEventListener('resize', this.onResize); // actualiza al redimensionar
  }

  ngOnDestroy(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    window.removeEventListener('resize', this.onResize);
  }

  // Mide <app-header> y guarda su altura en la var CSS --header-h
  private setHeaderVar(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const headerEl = document.querySelector('app-header') as HTMLElement | null;
    const h = headerEl ? Math.ceil(headerEl.getBoundingClientRect().height) : 160;
    document.documentElement.style.setProperty('--header-h', `${h}px`);
  }
}
