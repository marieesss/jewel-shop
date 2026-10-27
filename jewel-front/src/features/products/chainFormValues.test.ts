import { describe, expect, it } from 'vitest';
import type { ChainDto } from '../../api/products';
import type { ProductCommonValues } from './productForm';
import {
  INITIAL_ROLL_LENGTH_MESSAGE,
  chainCommonFromProduct,
  chainSpecificFromProduct,
  toCreateChainPayload,
  toUpdateChainPayload,
  validateChain,
  type ChainSpecificValues,
} from './chainFormValues';

const validCommon: ProductCommonValues = {
  name: 'Chaîne forçat',
  description: 'Maille fine',
  color: 'Gold',
  url: '',
  cost: '4.5',
  price: '20',
  tagIds: [2, 5],
};

const validSpecific: ChainSpecificValues = {
  claspCost: '0.80',
  claspPrice: '3',
  initialRollLengthCm: '500',
};

const CREATE = true;
const EDIT = false;

describe('validateChain', () => {
  it('accepte un formulaire de création complet', () => {
    expect(validateChain(validCommon, validSpecific, CREATE)).toEqual({});
  });

  it('accepte des montants à zéro', () => {
    const errors = validateChain(
      { ...validCommon, cost: '0', price: '0' },
      { ...validSpecific, claspCost: '0', claspPrice: '0' },
      CREATE
    );
    expect(errors).toEqual({});
  });

  it('refuse un prix au mètre négatif', () => {
    const errors = validateChain({ ...validCommon, price: '-1' }, validSpecific, CREATE);
    expect(errors.price).toBe('Le prix doit être positif.');
  });

  it('refuse un coût au mètre négatif', () => {
    const errors = validateChain({ ...validCommon, cost: '-0.01' }, validSpecific, CREATE);
    expect(errors.cost).toBe('Le coût doit être positif.');
  });

  it('exige le coût et le prix au mètre', () => {
    const errors = validateChain({ ...validCommon, cost: '', price: ' ' }, validSpecific, CREATE);
    expect(errors.cost).toBe('Le coût au mètre est requis.');
    expect(errors.price).toBe('Le prix au mètre est requis.');
  });

  it('refuse un prix du fermoir négatif', () => {
    const errors = validateChain(validCommon, { ...validSpecific, claspPrice: '-2' }, CREATE);
    expect(errors.claspPrice).toBe('Le prix doit être positif.');
  });

  it('refuse un coût du fermoir négatif', () => {
    const errors = validateChain(validCommon, { ...validSpecific, claspCost: '-2' }, CREATE);
    expect(errors.claspCost).toBe('Le coût doit être positif.');
  });

  it('exige le coût et le prix du fermoir', () => {
    const errors = validateChain(
      validCommon,
      { ...validSpecific, claspCost: '', claspPrice: '' },
      CREATE
    );
    expect(errors.claspCost).toBe('Le coût du fermoir est requis.');
    expect(errors.claspPrice).toBe('Le prix du fermoir est requis.');
  });

  it.each(['', '0', '-5', '45.5'])(
    'refuse la longueur de rouleau « %s » à la création',
    (initialRollLengthCm) => {
      const errors = validateChain(validCommon, { ...validSpecific, initialRollLengthCm }, CREATE);
      expect(errors.initialRollLengthCm).toBe(INITIAL_ROLL_LENGTH_MESSAGE);
    }
  );

  it('ne demande pas de longueur de rouleau en modification', () => {
    const errors = validateChain(validCommon, { ...validSpecific, initialRollLengthCm: '' }, EDIT);
    expect(errors).toEqual({});
  });

  it('accepte une saisie avec une virgule', () => {
    const errors = validateChain({ ...validCommon, price: '19,90' }, validSpecific, CREATE);
    expect(errors).toEqual({});
  });
});

describe('toCreateChainPayload', () => {
  it('envoie les prix au mètre, le fermoir et la longueur du rouleau', () => {
    expect(toCreateChainPayload({ ...validCommon, name: '  Forçat  ' }, validSpecific)).toEqual({
      name: 'Forçat',
      description: 'Maille fine',
      color: 'Gold',
      url: null,
      costPerMeter: 4.5,
      pricePerMeter: 20,
      claspCost: 0.8,
      claspPrice: 3,
      tagIds: [2, 5],
      initialRollLengthCm: 500,
    });
  });

  it("n'envoie plus l'ancien format", () => {
    const payload = toCreateChainPayload(validCommon, validSpecific);
    expect(payload).not.toHaveProperty('cost');
    expect(payload).not.toHaveProperty('price');
    expect(payload).not.toHaveProperty('length');
  });
});

describe('toUpdateChainPayload', () => {
  it('envoie les prix au mètre et le fermoir, avec les textes nettoyés', () => {
    expect(
      toUpdateChainPayload({ ...validCommon, name: '  Forçat  ', url: '   ' }, validSpecific)
    ).toEqual({
      name: 'Forçat',
      description: 'Maille fine',
      color: 'Gold',
      url: null,
      costPerMeter: 4.5,
      pricePerMeter: 20,
      claspCost: 0.8,
      claspPrice: 3,
      tagIds: [2, 5],
    });
  });

  it('envoie une liste de tags vide pour retirer tous les tags', () => {
    expect(toUpdateChainPayload({ ...validCommon, tagIds: [] }, validSpecific).tagIds).toEqual([]);
  });

  it("n'envoie ni l'ancien format ni de rouleau", () => {
    const payload = toUpdateChainPayload(validCommon, validSpecific);
    expect(payload).not.toHaveProperty('cost');
    expect(payload).not.toHaveProperty('price');
    expect(payload).not.toHaveProperty('length');
    expect(payload).not.toHaveProperty('initialRollLengthCm');
  });
});

describe('pré-remplissage depuis une chaîne', () => {
  const chain: ChainDto = {
    id: 7,
    name: 'Vénitienne',
    description: '',
    color: 'Silver',
    imageUrl: null,
    url: 'https://exemple.com/chaine',
    cost: 10.5,
    price: 44,
    length: 200,
    costPerMeter: 5.25,
    pricePerMeter: 22,
    claspCost: 1,
    claspPrice: 4.5,
    stockCm: 1200,
    tags: [
      { id: 4, name: 'Mer', slug: 'mer', category: 'Theme' },
      { id: 9, name: 'Argenté', slug: 'argente', category: 'Color' },
    ],
  };

  it('reprend les prix au mètre dans le coût et le prix', () => {
    expect(chainCommonFromProduct(chain)).toEqual({
      name: 'Vénitienne',
      description: '',
      color: 'Silver',
      url: 'https://exemple.com/chaine',
      cost: '5.25',
      price: '22',
      tagIds: [4, 9],
    });
  });

  it('reprend le fermoir et laisse la longueur du rouleau vide', () => {
    expect(chainSpecificFromProduct(chain)).toEqual({
      claspCost: '1',
      claspPrice: '4.5',
      initialRollLengthCm: '',
    });
  });

  it('laisse vides les prix au mètre absents', () => {
    const values = chainCommonFromProduct({ ...chain, costPerMeter: null, pricePerMeter: null });
    expect(values.cost).toBe('');
    expect(values.price).toBe('');
  });
});
