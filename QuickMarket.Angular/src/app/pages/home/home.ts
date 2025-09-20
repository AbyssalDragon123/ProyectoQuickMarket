import { Component, ElementRef, ViewChild, AfterViewInit, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';

type Category = { id: number; name: string; icon: string };

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements AfterViewInit {
  @ViewChild('list', { static: true }) listRef!: ElementRef<HTMLDivElement>;

  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);

  // 13 categorías
  all: Category[] = [
    { id: 1,  name: 'Higiene y Belleza',         icon: '/iconos/hand-wash.png' },
    { id: 2,  name: 'Lácteos y Huevos',          icon: '/iconos/milk.png' },
    { id: 3,  name: 'Farmacia',                  icon: '/iconos/medicine.png' },
    { id: 4,  name: 'Mascotas',                  icon: '/iconos/pet-food.png' },
    { id: 5,  name: 'Alimentos Congelados',      icon: '/iconos/steak.png' },
    { id: 6,  name: 'Carnes y Pescados',         icon: '/iconos/proteins.png' },
    { id: 7,  name: 'Panadería',                 icon: '/iconos/breads.png' },
    { id: 8,  name: 'Licores',                   icon: '/iconos/cheers.png' },
    { id: 9,  name: 'Embutidos',                 icon: '/iconos/sausage.png' },
    { id:10,  name: 'Juguetes',                  icon: '/iconos/toys.png' },
    { id:11,  name: 'Frutas y Verduras',         icon: '/iconos/healthy-food.png' },
    { id:12,  name: 'Deportes',                  icon: '/iconos/basket-ball.png' },
    { id:13,  name: 'Abarrotes',                 icon: '/iconos/basket.png' }
  ];

  // Render y medidas (sincronizar con CSS)
  looped: Category[] = [...this.all];
  private readonly catWidth = 120;   // --cat-w
  private readonly gap = 20;         // gap entre .cat
  private readonly step = this.catWidth + this.gap;
  private readonly centerIndex = 4;  // de 9 visibles, centro es 4 (0..8)

  // ✅ Inicializa activo desde YA (evita tocarlo en AfterViewInit)
  activeId = this.looped[this.centerIndex].id;
  animating = false;

  ngAfterViewInit(): void {
    // (Intencionalmente vacío)
    // ❌ No reasignar activeId aquí para evitar NG0100
  }

  // === animación 1 paso y rotación invisible (loop infinito recto) ===
  private animateStep(dir: 1 | -1): Promise<void> {
    return new Promise(resolve => {
      const list = this.listRef.nativeElement;
      const distance = dir > 0 ? -this.step : this.step; // derecha: muevo lista a la izq.

      this.animating = true;
      list.style.willChange = 'transform';
      list.style.transition = 'transform 0.35s ease';
      list.style.transform = `translateX(${distance}px)`;

      const onEnd = () => {
        list.removeEventListener('transitionend', onEnd);

        // Quitar transición para reacomodar sin parpadeo
        list.style.transition = 'none';
        list.style.transform = 'translateX(0)';

        // Rotar arreglo
        if (dir > 0) {
          const first = this.looped.shift()!;
          this.looped.push(first);
        } else {
          const last = this.looped.pop()!;
          this.looped.unshift(last);
        }

        // ✅ Actualizar activa con la categoría que quedó al centro
        this.activeId = this.looped[this.centerIndex].id;

        // Reflow y limpieza
        // eslint-disable-next-line @typescript-eslint/no-unused-expressions
        list.offsetHeight;
        list.style.willChange = 'auto';
        this.animating = false;
        resolve();
      };

      list.addEventListener('transitionend', onEnd, { once: true });
    });
  }

  go(dir: number): void {
    if (!this.isBrowser || this.animating) return;
    if (dir !== 1 && dir !== -1) return;
    void this.animateStep(dir as 1 | -1);
  }

  // === Llevar una categoría al centro con pasos mínimos, sin cortes ===
  async onCategoryClick(cat: Category) {
    if (this.animating) return;

    const currentCenterId = this.looped[this.centerIndex]?.id;
    if (currentCenterId === cat.id) {
      // ✅ Ya está al centro: marcarla activa
      this.activeId = cat.id;
      return;
    }

    // Calcular desplazamiento mínimo hasta que el seleccionado quede en centerIndex
    const from = this.looped.findIndex(c => c.id === cat.id);
    if (from === -1) return;

    let delta = from - this.centerIndex; // >0: hay que mover a la izquierda; <0: a la derecha
    // Elegir dirección con menos pasos (carrusel circular)
    if (Math.abs(delta) > this.looped.length / 2) {
      delta = delta > 0 ? delta - this.looped.length : delta + this.looped.length;
    }

    const dir: 1 | -1 = delta > 0 ? 1 : -1; // 1 = avanzar derecha (lista se mueve izq)
    const steps = Math.abs(delta);

    // Animar paso a paso hasta centrar
    this.animating = true;
    for (let i = 0; i < steps; i++) {
      await this.animateStep(dir);
    }
    this.animating = false;

    // ✅ Asegurar que la activa sea la clickeada (que ahora está al centro)
    this.activeId = cat.id;
  }

  trackById = (_: number, c: Category) => c.id;
}
