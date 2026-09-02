# CK-Globalization-Engine

[![Licence](https://img.shields.io/github/license/signature-opensource/CK-Globalization-Engine.svg)](LICENSE)

Composes the backend translations of every package at build time, so that an application starts with its
per-culture formats already registered.

| Package | Description | Latest stable |
|---------|-------------|---------------|
| [CK.Globalization.StObj](CK.Globalization.StObj/README.md) | The `[LocalePackage]` attribute that marks a package as contributing translations. | [![nuget](https://img.shields.io/nuget/v/CK.Globalization.StObj.svg?label=CK.Globalization.StObj)](https://www.nuget.org/packages/CK.Globalization.StObj/) |
| [CK.Globalization.Engine](CK.Globalization.Engine/README.md) | The CKSetup aspect that merges the locales and generates their registration. | [![nuget](https://img.shields.io/nuget/v/CK.Globalization.Engine.svg?label=CK.Globalization.Engine)](https://www.nuget.org/packages/CK.Globalization.Engine/) |
