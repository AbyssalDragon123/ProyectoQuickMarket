import { Component, inject, PLATFORM_ID, AfterViewInit } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-terms',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './terms.html',
  styleUrls: ['./terms.css']
})
export class Terms implements AfterViewInit {
  private platformId = inject(PLATFORM_ID);
  private route = inject(ActivatedRoute);

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    setTimeout(() => {
      const frag = this.route.snapshot.fragment;
      if (frag) this.scrollTo(frag);
    });
  }

  scrollTo(id: string, evt?: Event) {
    evt?.preventDefault();
    if (!isPlatformBrowser(this.platformId)) return;
    const headerEl = document.querySelector('app-header') as HTMLElement | null;
    const headerH = headerEl ? headerEl.getBoundingClientRect().height : 0;
    const el = document.getElementById(id);
    if (!el) return;
    const top = window.scrollY + el.getBoundingClientRect().top - headerH - 12;
    window.scrollTo({ top, behavior: 'smooth' });
  }
}
