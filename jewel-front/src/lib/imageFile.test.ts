import { describe, expect, it } from 'vitest';
import { IMAGE_ACCEPT, validateImageFile } from './imageFile';

const CONTENT_TYPE_MESSAGE = 'Le fichier doit être une image JPEG, PNG ou WEBP.';
const EXTENSION_MESSAGE = "L'extension du fichier ne correspond pas à son format.";
const MAX_SIZE_MESSAGE = "L'image ne doit pas dépasser 5 Mo.";
const MAX_SIZE_BYTES = 5 * 1024 * 1024;

const image = (name: string, type: string, size = 1024) => ({ name, type, size });

describe('validateImageFile', () => {
  it.each([
    ['photo.jpg', 'image/jpeg'],
    ['photo.JPEG', 'image/jpeg'],
    ['photo.png', 'image/png'],
    ['photo.webp', 'image/webp'],
  ])('accepte %s (%s)', (name, type) => {
    expect(validateImageFile(image(name, type))).toEqual([]);
  });

  it.each([
    ['page.html', 'text/html'],
    ['anim.gif', 'image/gif'],
    ['setup.exe', ''],
  ])('refuse le type de %s', (name, type) => {
    expect(validateImageFile(image(name, type))).toEqual([CONTENT_TYPE_MESSAGE]);
  });

  it.each([
    ['photo.png', 'image/jpeg'],
    ['page.html', 'image/png'],
    ['photo', 'image/webp'],
  ])('refuse l’extension de %s déclaré en %s', (name, type) => {
    expect(validateImageFile(image(name, type))).toEqual([EXTENSION_MESSAGE]);
  });

  it('accepte une image de exactement 5 Mo', () => {
    expect(validateImageFile(image('photo.jpg', 'image/jpeg', MAX_SIZE_BYTES))).toEqual([]);
  });

  it('refuse une image de plus de 5 Mo', () => {
    expect(validateImageFile(image('photo.jpg', 'image/jpeg', MAX_SIZE_BYTES + 1))).toEqual([
      MAX_SIZE_MESSAGE,
    ]);
  });

  it('ne propose que les types acceptés par l’API', () => {
    expect(IMAGE_ACCEPT).toBe('image/jpeg,image/png,image/webp');
  });
});
