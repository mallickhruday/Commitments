import { Component } from "@angular/core";
import { Subject, Observable, BehaviorSubject } from "rxjs";
import { takeUntil, tap, map } from "rxjs/operators";
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from "@angular/router";
import { CardService } from "./card.service";
import { Card } from "./card.model";
import { deepCopy } from "../core/deep-copy";
import { GridApi, ColDef } from "ag-grid";
import { CheckboxCellComponent } from "../shared/checkbox-cell.component";
import { DeleteCellComponent } from "../shared/delete-cell.component";
import { EditCellComponent } from "../shared/edit-cell.component";
import { EditCardOverlay } from "../cards/edit-card-overlay";

@Component({
  templateUrl: "./cards-page.component.html",
  styleUrls: ["./cards-page.component.css"],
  selector: "app-cards-page"
})
export class CardsPageComponent { 
  constructor(
    private _cardService: CardService,
    private _editCardOverlay: EditCardOverlay,
    private _router: Router
  ) { }

  ngOnInit() {
    this._cardService.get()
      .pipe(map(x => this.cards$.next(x)))
      .subscribe();
  }

  public onDestroy: Subject<void> = new Subject<void>();

  public cards$: BehaviorSubject<Array<Card>> = new BehaviorSubject([]);

  ngOnDestroy() {
    this.onDestroy.next();    
  }

  public handleRemoveClick($event) {
    const cards: Array<Card> = [...this.cards$.value];
    const index = cards.findIndex(x => x.cardId == $event.data.cardId);
    cards.splice(index, 1);
    this.cards$.next(cards);

    this._cardService.remove({ card: $event.data })
      .pipe(takeUntil(this.onDestroy))
      .subscribe();
  }

  public handleEditClick($event) {    
    this._editCardOverlay.create()
      .pipe(takeUntil(this.onDestroy))
      .subscribe();
  }

  public addOrUpdate(card: Card) {
    if (!card) return;

    let cards = [...this.cards$.value];
    const i = cards.findIndex((t) => t.cardId == card.cardId);
    const _ = i < 0 ? cards.push(card) : cards[i] = card;    
    this.cards$.next(cards);
  }

  public columnDefs: Array<ColDef> = [
    { headerName: "Name", field: "name" },
    { cellRenderer: "editRenderer", onCellClicked: $event => this.handleEditClick($event), width: 30 },
    { cellRenderer: "deleteRenderer", onCellClicked: $event => this.handleRemoveClick($event), width: 30 }
  ];

  public frameworkComponents: any = {
    checkboxRenderer: CheckboxCellComponent,
    deleteRenderer: DeleteCellComponent,
    editRenderer: EditCellComponent
  };

  private _gridApi: GridApi;

  public onGridReady(params) {
    this._gridApi = params.api;
    this._gridApi.sizeColumnsToFit();
  }
}
