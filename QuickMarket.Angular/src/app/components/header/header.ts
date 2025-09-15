import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class Header {
  activeCategory: string | null = null;

  setActiveCategory(category: string) {
    this.activeCategory = category;
  }

  clearActiveCategory() {
    this.activeCategory = null;
  }
}
