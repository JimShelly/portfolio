import { LitElement, html, property, customElement } from 'lit-element';
import { AddressType } from '../common/addresstypes.enum';

@customElement('address-view')
export class AddressView extends LitElement {
  static get properties() {
    return {
      addressType: { type: AddressType },
      name: { type: Array },
      street1: { type: String },
      street2: { type: String },
      city: { type: String },
      state: { type: String },
      zipCode: { type: String },
      phone: { type: String },
    };
  }

  render() {
    return html`
      <div class =
    `;
  }
}
