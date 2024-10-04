import React, { useCallback, useState } from 'react';
import FieldSet from 'Components/FieldSet';
import SelectInput from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, sizes } from 'Helpers/Props';
import NamingConfig from 'typings/Settings/NamingConfig';
import translate from 'Utilities/String/translate';
import NamingOption from './NamingOption';
import TokenCase from './TokenCase';
import TokenSeparator from './TokenSeparator';
import styles from './NamingModal.css';

const separatorOptions: { key: TokenSeparator; value: string }[] = [
  {
    key: ' ',
    get value() {
      return `${translate('Space')} ( )`;
    },
  },
  {
    key: '.',
    get value() {
      return `${translate('Period')} (.)`;
    },
  },
  {
    key: '_',
    get value() {
      return `${translate('Underscore')} (_)`;
    },
  },
  {
    key: '-',
    get value() {
      return `${translate('Dash')} (-)`;
    },
  },
];

const caseOptions: { key: TokenCase; value: string }[] = [
  {
    key: 'title',
    get value() {
      return translate('DefaultCase');
    },
  },
  {
    key: 'lower',
    get value() {
      return translate('Lowercase');
    },
  },
  {
    key: 'upper',
    get value() {
      return translate('Uppercase');
    },
  },
];

const fileNameTokens = [
  {
    token:
      '{Movie Title} ({Release Year}) - {Edition Tags }{[Custom Formats]}{[Quality Full]}{-Release Group}',
    example:
      'The Movie - Title (2010) - Ultimate Extended Edition [Surround Sound x264][Bluray-1080p Proper]-EVOLVE',
  },
  {
    token:
      '{Movie CleanTitle} {Release Year} - {Edition Tags }{[Custom Formats]}{[Quality Full]}{-Release Group}',
    example:
      'The Movie Title 2010 - Ultimate Extended Edition [Surround Sound x264][Bluray-1080p Proper]-EVOLVE',
  },
  {
    token:
      '{Movie.CleanTitle}{.Release.Year}{.Edition.Tags}{.Custom.Formats}{.Quality.Full}{-Release Group}',
    example:
      'The.Movie.Title.2010.Ultimate.Extended.Edition.Surround.Sound.x264.Bluray-1080p.Proper-EVOLVE',
  },
];

const movieTokens = [
  { token: '{Movie Title}', example: "Movie's Title", footNote: true },
  { token: '{Movie Title:DE}', example: 'Titel des Films', footNote: true },
  { token: '{Movie CleanTitle}', example: 'Movies Title', footNote: true },
  {
    token: '{Movie CleanTitle:DE}',
    example: 'Titel des Films',
    footNote: true,
  },
  { token: '{Movie TitleThe}', example: "Movie's Title, The", footNote: true },
  {
    token: '{Movie CleanTitleThe}',
    example: 'Movies Title, The',
    footNote: true,
  },
  { token: '{Movie OriginalTitle}', example: 'Τίτλος ταινίας', footNote: true },
  {
    token: '{Movie CleanOriginalTitle}',
    example: 'Τίτλος ταινίας',
    footNote: true,
  },
  { token: '{Movie TitleFirstCharacter}', example: 'M' },
  { token: '{Movie TitleFirstCharacter:DE}', example: 'T' },
  {
    token: '{Movie Collection}',
    example: 'The Movie Collection',
    footNote: true,
  },
  { token: '{Movie Certification}', example: 'R' },
  { token: '{Release Year}', example: '2009' },
];

const movieIdTokens = [
  { token: '{ImdbId}', example: 'tt12345' },
  { token: '{TmdbId}', example: '123456' },
];

const qualityTokens = [
  { token: '{Quality Full}', example: 'HDTV-720p Proper' },
  { token: '{Quality Title}', example: 'HDTV-720p' },
];

const mediaInfoTokens = [
  { token: '{MediaInfo Simple}', example: 'x264 DTS' },
  { token: '{MediaInfo Full}', example: 'x264 DTS [EN+DE]', footNote: true },

  { token: '{MediaInfo AudioCodec}', example: 'DTS' },
  { token: '{MediaInfo AudioChannels}', example: '5.1' },
  { token: '{MediaInfo AudioLanguages}', example: '[EN+DE]', footNote: true },
  { token: '{MediaInfo SubtitleLanguages}', example: '[DE]', footNote: true },

  { token: '{MediaInfo VideoCodec}', example: 'x264' },
  { token: '{MediaInfo VideoBitDepth}', example: '10' },
  { token: '{MediaInfo VideoDynamicRange}', example: 'HDR' },
  { token: '{MediaInfo VideoDynamicRangeType}', example: 'DV HDR10' },
  { token: '{MediaInfo 3D}', example: '3D' },
];

const releaseGroupTokens = [
  { token: '{Release Group}', example: 'Rls Grp', footNote: true },
];

const editionTokens = [
  { token: '{Edition Tags}', example: 'IMAX', footNote: true },
];

const customFormatTokens = [
  { token: '{Custom Formats}', example: 'Surround Sound x264' },
  { token: '{Custom Format:FormatName}', example: 'AMZN' },
];

const originalTokens = [
  { token: '{Original Title}', example: 'Movie.Title.HDTV.x264-EVOLVE' },
  { token: '{Original Filename}', example: 'movie title hdtv.x264-Evolve' },
];

interface NamingModalProps {
  isOpen: boolean;
  name: keyof Pick<NamingConfig, 'standardMovieFormat' | 'movieFolderFormat'>;
  value: string;
  movie?: boolean;
  additional?: boolean;
  onInputChange: ({ name, value }: { name: string; value: string }) => void;
  onModalClose: () => void;
}

function NamingModal(props: NamingModalProps) {
  const {
    isOpen,
    name,
    value,
    movie = false,
    additional = false,
    onInputChange,
    onModalClose,
  } = props;

  const [tokenSeparator, setTokenSeparator] = useState<TokenSeparator>(' ');
  const [tokenCase, setTokenCase] = useState<TokenCase>('title');
  const [selectionStart, setSelectionStart] = useState<number | null>(null);
  const [selectionEnd, setSelectionEnd] = useState<number | null>(null);

  const handleTokenSeparatorChange = useCallback(
    ({ value }: { value: TokenSeparator }) => {
      setTokenSeparator(value);
    },
    [setTokenSeparator]
  );

  const handleTokenCaseChange = useCallback(
    ({ value }: { value: TokenCase }) => {
      setTokenCase(value);
    },
    [setTokenCase]
  );

  const handleInputSelectionChange = useCallback(
    (selectionStart: number, selectionEnd: number) => {
      setSelectionStart(selectionStart);
      setSelectionEnd(selectionEnd);
    },
    [setSelectionStart, setSelectionEnd]
  );

  const handleOptionPress = useCallback(
    ({
      isFullFilename,
      tokenValue,
    }: {
      isFullFilename: boolean;
      tokenValue: string;
    }) => {
      if (isFullFilename) {
        onInputChange({ name, value: tokenValue });
      } else if (selectionStart == null || selectionEnd == null) {
        onInputChange({
          name,
          value: `${value}${tokenValue}`,
        });
      } else {
        const start = value.substring(0, selectionStart);
        const end = value.substring(selectionEnd);
        const newValue = `${start}${tokenValue}${end}`;

        onInputChange({ name, value: newValue });

        setSelectionStart(newValue.length - 1);
        setSelectionEnd(newValue.length - 1);
      }
    },
    [name, value, selectionEnd, selectionStart, onInputChange]
  );

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {movie ? translate('FileNameTokens') : translate('FolderNameTokens')}
        </ModalHeader>

        <ModalBody>
          <div className={styles.namingSelectContainer}>
            <SelectInput
              className={styles.namingSelect}
              name="separator"
              value={tokenSeparator}
              values={separatorOptions}
              onChange={handleTokenSeparatorChange}
            />

            <SelectInput
              className={styles.namingSelect}
              name="case"
              value={tokenCase}
              values={caseOptions}
              onChange={handleTokenCaseChange}
            />
          </div>

          {movie ? (
            <FieldSet legend={translate('FileNames')}>
              <div className={styles.groups}>
                {fileNameTokens.map(({ token, example }) => (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    isFullFilename={true}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    size={sizes.LARGE}
                    onPress={handleOptionPress}
                  />
                ))}
              </div>
            </FieldSet>
          ) : null}

          <FieldSet legend={translate('Movie')}>
            <div className={styles.groups}>
              {movieTokens.map(({ token, example, footNote }) => {
                return (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    footNote={footNote}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    onPress={handleOptionPress}
                  />
                );
              })}
            </div>

            <div className={styles.footNote}>
              <Icon className={styles.icon} name={icons.FOOTNOTE} />
              <InlineMarkdown data={translate('MovieFootNote')} />
            </div>
          </FieldSet>

          <FieldSet legend={translate('MovieID')}>
            <div className={styles.groups}>
              {movieIdTokens.map(({ token, example }) => {
                return (
                  <NamingOption
                    key={token}
                    token={token}
                    example={example}
                    tokenSeparator={tokenSeparator}
                    tokenCase={tokenCase}
                    onPress={handleOptionPress}
                  />
                );
              })}
            </div>
          </FieldSet>

          {additional ? (
            <div>
              <FieldSet legend={translate('Quality')}>
                <div className={styles.groups}>
                  {qualityTokens.map(({ token, example }) => {
                    return (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={handleOptionPress}
                      />
                    );
                  })}
                </div>
              </FieldSet>

              <FieldSet legend={translate('MediaInfo')}>
                <div className={styles.groups}>
                  {mediaInfoTokens.map(({ token, example, footNote }) => {
                    return (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        footNote={footNote}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={handleOptionPress}
                      />
                    );
                  })}
                </div>

                <div className={styles.footNote}>
                  <Icon className={styles.icon} name={icons.FOOTNOTE} />
                  <InlineMarkdown data={translate('MediaInfoFootNote')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('ReleaseGroup')}>
                <div className={styles.groups}>
                  {releaseGroupTokens.map(({ token, example, footNote }) => {
                    return (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        footNote={footNote}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={handleOptionPress}
                      />
                    );
                  })}
                </div>

                <div className={styles.footNote}>
                  <Icon className={styles.icon} name={icons.FOOTNOTE} />
                  <InlineMarkdown data={translate('ReleaseGroupFootNote')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('Edition')}>
                <div className={styles.groups}>
                  {editionTokens.map(({ token, example, footNote }) => {
                    return (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        footNote={footNote}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={handleOptionPress}
                      />
                    );
                  })}
                </div>

                <div className={styles.footNote}>
                  <Icon className={styles.icon} name={icons.FOOTNOTE} />
                  <InlineMarkdown data={translate('EditionFootNote')} />
                </div>
              </FieldSet>

              <FieldSet legend={translate('CustomFormats')}>
                <div className={styles.groups}>
                  {customFormatTokens.map(({ token, example }) => {
                    return (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        onPress={handleOptionPress}
                      />
                    );
                  })}
                </div>
              </FieldSet>

              <FieldSet legend={translate('Original')}>
                <div className={styles.groups}>
                  {originalTokens.map(({ token, example }) => {
                    return (
                      <NamingOption
                        key={token}
                        token={token}
                        example={example}
                        tokenSeparator={tokenSeparator}
                        tokenCase={tokenCase}
                        size={sizes.LARGE}
                        onPress={handleOptionPress}
                      />
                    );
                  })}
                </div>
              </FieldSet>
            </div>
          ) : null}
        </ModalBody>

        <ModalFooter>
          <TextInput
            name={name}
            value={value}
            onChange={onInputChange}
            onSelectionChange={handleInputSelectionChange}
          />

          <Button onPress={onModalClose}>{translate('Close')}</Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default NamingModal;
