import { Component, inject, PLATFORM_ID, AfterViewInit } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-returns',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './returns.html',
  styleUrls: ['./returns.css']
})
export class Returns implements AfterViewInit {
  private platformId = inject(PLATFORM_ID);
  private route = inject(ActivatedRoute);

  ngAfterViewInit(): void {
    // Si entras con /politica-devoluciones#algo, re-ajusta con offset del header
    if (!isPlatformBrowser(this.platformId)) return;
    setTimeout(() => {
      const frag = this.route.snapshot.fragment;
      if (frag) this.scrollTo(frag);
    });
  }

  scrollTo(id: string, evt?: Event) {
    evt?.preventDefault(); // evita navegación del <a>
    if (!isPlatformBrowser(this.platformId)) return;

    // Medir altura real del header (incluye barra de promos porque es parte del header)
    const headerEl = document.querySelector('app-header') as HTMLElement | null;
    const headerH = headerEl ? headerEl.getBoundingClientRect().height : 0;

    // Buscar destino y calcular posición con offset
    const el = document.getElementById(id);
    if (!el) return;

    const top = window.scrollY + el.getBoundingClientRect().top - headerH - 12; // margen extra
    window.scrollTo({ top, behavior: 'smooth' });
  }
}
