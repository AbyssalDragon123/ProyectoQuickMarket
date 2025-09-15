import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Product } from './pages/product/product';
import { Contact } from './pages/contact/contact';
import { About } from './pages/about/about';

export const routes: Routes = [
    { path: '', redirectTo: '/home', pathMatch: 'full' },
    { path: 'home', component: Home },
    { path: 'product', component: Product },
    { path: 'contact', component: Contact },
    { path: 'nosotros', component: About },
    { path: '**', redirectTo: '/home' }
];
