import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Product } from './pages/product/product';
import { Contact } from './pages/contact/contact';
import { About } from './pages/about/about';

// IMPORTS de las páginas de políticas (sin .component)
import { Terms } from './pages/policies/terms/terms';
import { Privacy } from './pages/policies/privacy/privacy';
import { Returns } from './pages/policies/returns/returns';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },

  { path: 'home', component: Home, title: 'Inicio' },
  { path: 'product', component: Product, title: 'Productos' },
  { path: 'contact', component: Contact, title: 'Contácto' },
  { path: 'nosotros', component: About, title: 'Nosotros' },

  // Rutas nuevas
  { path: 'terminos-condiciones', component: Terms, title: 'Términos y Condiciones' },
  { path: 'politica-privacidad', component: Privacy, title: 'Política de Privacidad' },
  { path: 'politica-devoluciones', component: Returns, title: 'Política de Devoluciones' },

  { path: '**', redirectTo: '/home' }
];
