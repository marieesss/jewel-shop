import type { ChainDto, CreateChainPayload, UpdateChainPayload } from '../../api/products';
import {
  COST_MESSAGE,
  PRICE_MESSAGE,
  toNumber,
  validateAmount,
  validateCommon,
  type FieldErrors,
  type ProductCommonValues,
} from './productForm';

/**
 * Champs propres à la chaîne. Les champs communs `cost` et `price` portent le coût et le prix
 * au mètre ; la longueur du rouleau n'est saisie qu'à la création.
 */
export interface ChainSpecificValues {
  claspCost: string;
  claspPrice: string;
  initialRollLengthCm: string;
}

export type ChainField = keyof ProductCommonValues | keyof ChainSpecificValues;
export type ChainErrors = FieldErrors<ChainField>;

export const INITIAL_ROLL_LENGTH_MESSAGE = 'La longueur du rouleau doit être supérieure à 0.';

const METER_PRICE_REQUIRED_MESSAGES = {
  cost: 'Le coût au mètre est requis.',
  price: 'Le prix au mètre est requis.',
};

export const emptyChainSpecific: ChainSpecificValues = {
  claspCost: '',
  claspPrice: '',
  initialRollLengthCm: '',
};

const toInput = (value: number | null | undefined): string =>
  value === null || value === undefined ? '' : String(value);

/** Pré-remplit les champs communs d'une chaîne existante, avec ses prix au mètre. */
export function chainCommonFromProduct(chain: ChainDto): ProductCommonValues {
  return {
    name: chain.name,
    description: chain.description,
    color: chain.color as ProductCommonValues['color'],
    url: chain.url ?? '',
    cost: toInput(chain.costPerMeter),
    price: toInput(chain.pricePerMeter),
  };
}

/** Pré-remplit les champs du fermoir d'une chaîne existante. */
export function chainSpecificFromProduct(chain: ChainDto): ChainSpecificValues {
  return {
    claspCost: toInput(chain.claspCost),
    claspPrice: toInput(chain.claspPrice),
    initialRollLengthCm: '',
  };
}

function validateInitialRollLength(raw: string) {
  const lengthCm = toNumber(raw);
  const isPositiveInteger = Number.isInteger(lengthCm) && lengthCm > 0;
  return isPositiveInteger ? undefined : INITIAL_ROLL_LENGTH_MESSAGE;
}

/**
 * Valide le formulaire de chaîne (miroir de `ChainPricesValidator` et de
 * `CreateChainCommandValidator` côté back). La longueur du rouleau n'est contrôlée qu'à la création.
 */
export function validateChain(
  common: ProductCommonValues,
  specific: ChainSpecificValues,
  isCreate: boolean
): ChainErrors {
  const errors: ChainErrors = validateCommon(common, METER_PRICE_REQUIRED_MESSAGES);

  const claspCostError = validateAmount(
    specific.claspCost,
    'Le coût du fermoir est requis.',
    COST_MESSAGE
  );
  if (claspCostError) errors.claspCost = claspCostError;

  const claspPriceError = validateAmount(
    specific.claspPrice,
    'Le prix du fermoir est requis.',
    PRICE_MESSAGE
  );
  if (claspPriceError) errors.claspPrice = claspPriceError;

  if (isCreate) {
    const rollError = validateInitialRollLength(specific.initialRollLengthCm);
    if (rollError) errors.initialRollLengthCm = rollError;
  }

  return errors;
}

/** Corps de `PUT /api/chains/{id}` : nouveau format uniquement (ni `cost`, ni `price`, ni `length`). */
export function toUpdateChainPayload(
  common: ProductCommonValues,
  specific: ChainSpecificValues
): UpdateChainPayload {
  return {
    name: common.name.trim(),
    description: common.description.trim(),
    color: common.color,
    url: common.url.trim() || null,
    costPerMeter: toNumber(common.cost),
    pricePerMeter: toNumber(common.price),
    claspCost: toNumber(specific.claspCost),
    claspPrice: toNumber(specific.claspPrice),
  };
}

/** Corps de `POST /api/chains` : le nouveau format et la longueur du premier rouleau. */
export function toCreateChainPayload(
  common: ProductCommonValues,
  specific: ChainSpecificValues
): CreateChainPayload {
  return {
    ...toUpdateChainPayload(common, specific),
    initialRollLengthCm: toNumber(specific.initialRollLengthCm),
  };
}
