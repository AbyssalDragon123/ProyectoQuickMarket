import { Component, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';

import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    RouterModule, CommonModule, ReactiveFormsModule,
    MatToolbarModule, MatIconModule, MatButtonModule, MatMenuModule,
    MatBadgeModule, MatFormFieldModule, MatInputModule, MatDividerModule,
    MatTooltipModule
  ],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class Header {
  search = new FormControl('', { nonNullable: true });
  cartCount = signal(0);

  onSearch() {
    const q = this.search.value.trim();
    if (!q) return;
    // TODO: navega a tu ruta de búsqueda
    // this.router.navigate(['/buscar'], { queryParams: { q }});
    console.log('Buscar:', q);
  }
}
