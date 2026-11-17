var assetPath = "Assets/_Project/Art/UI/Generated/LifeIcons_GPT26_3K.png";
var importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
if (importer == null)
{
    throw new System.Exception("Life icon texture importer was not found.");
}

importer.textureType = UnityEditor.TextureImporterType.Sprite;
importer.spriteImportMode = UnityEditor.SpriteImportMode.Multiple;
importer.maxTextureSize = 4096;
importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
importer.mipmapEnabled = false;
importer.isReadable = true;
importer.alphaIsTransparency = true;
importer.filterMode = UnityEngine.FilterMode.Bilinear;
importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
importer.npotScale = UnityEditor.TextureImporterNPOTScale.None;
importer.SaveAndReimport();

var factories = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();
factories.Init();
var dataProvider = factories.GetSpriteEditorDataProviderFromObject(importer);
if (dataProvider == null)
{
    throw new System.Exception("Sprite editor data provider is not supported for this texture.");
}
dataProvider.InitSpriteEditorDataProvider();

var editCapabilityProvider = dataProvider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteFrameEditCapability>();
if (editCapabilityProvider == null)
{
    throw new System.Exception("Sprite edit capability provider is unavailable; no sprite metadata was changed.");
}
var capabilities = editCapabilityProvider.GetEditCapability();
if (!capabilities.HasCapability(UnityEditor.U2D.Sprites.EEditCapability.EditSpriteName) ||
    !capabilities.HasCapability(UnityEditor.U2D.Sprites.EEditCapability.EditSpriteRect) ||
    !capabilities.HasCapability(UnityEditor.U2D.Sprites.EEditCapability.CreateAndDeleteSprite))
{
    throw new System.Exception("Required sprite name, rectangle, or slicing capability is unavailable; no sprite metadata was changed.");
}

var textureDataProvider = dataProvider.GetDataProvider<UnityEditor.U2D.Sprites.ITextureDataProvider>();
var nameFileIdProvider = dataProvider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteNameFileIdDataProvider>();
if (textureDataProvider == null || nameFileIdProvider == null)
{
    throw new System.Exception("Required texture or sprite-name provider is unavailable; no sprite metadata was changed.");
}

textureDataProvider.GetTextureActualWidthAndHeight(out var sourceWidth, out var sourceHeight);
if (sourceWidth != 3072 || sourceHeight != 1536)
{
    throw new System.Exception("Unexpected source dimensions: " + sourceWidth + "x" + sourceHeight);
}
var readableTexture = textureDataProvider.GetReadableTexture2D();
if (readableTexture == null || readableTexture.width != sourceWidth || readableTexture.height != sourceHeight)
{
    throw new System.Exception("Could not read the full-resolution source texture; slicing was aborted.");
}

var pixels = readableTexture.GetPixels32();
var spriteNames = new string[]
{
    "Potion_Full",
    "Potion_Half",
    "Potion_Empty",
    "Shell_Open",
    "Shell_HalfClosed",
    "Shell_Closed"
};
var cellWidth = sourceWidth / spriteNames.Length;
var alphaThreshold = 8;
var spriteRects = new System.Collections.Generic.List<UnityEditor.SpriteRect>();
var nameFileIdPairs = new System.Collections.Generic.List<UnityEditor.SpriteNameFileIdPair>();

for (var index = 0; index < spriteNames.Length; index++)
{
    var cellMinX = index * cellWidth;
    var cellMaxX = cellMinX + cellWidth - 1;
    var minX = sourceWidth;
    var minY = sourceHeight;
    var maxX = -1;
    var maxY = -1;

    for (var y = 0; y < sourceHeight; y++)
    {
        var rowOffset = y * sourceWidth;
        for (var x = cellMinX; x <= cellMaxX; x++)
        {
            if (pixels[rowOffset + x].a < alphaThreshold)
            {
                continue;
            }

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
    }

    if (maxX < minX || maxY < minY)
    {
        throw new System.Exception("No visible pixels were found for " + spriteNames[index]);
    }

    var padding = 4;
    var rectMinX = System.Math.Max(cellMinX, minX - padding);
    var rectMinY = System.Math.Max(0, minY - padding);
    var rectMaxX = System.Math.Min(cellMaxX, maxX + padding);
    var rectMaxY = System.Math.Min(sourceHeight - 1, maxY + padding);
    var spriteId = UnityEditor.GUID.Generate();
    var spriteRect = new UnityEditor.SpriteRect
    {
        name = spriteNames[index],
        spriteID = spriteId,
        rect = new UnityEngine.Rect(rectMinX, rectMinY, rectMaxX - rectMinX + 1, rectMaxY - rectMinY + 1),
        alignment = (int)UnityEngine.SpriteAlignment.Center,
        pivot = new UnityEngine.Vector2(0.5f, 0.5f)
    };
    spriteRects.Add(spriteRect);
    nameFileIdPairs.Add(new UnityEditor.SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID));
}

dataProvider.SetSpriteRects(spriteRects.ToArray());
nameFileIdProvider.SetNameFileIdPairs(nameFileIdPairs);
dataProvider.Apply();

importer.isReadable = false;
importer.SaveAndReimport();

var importedAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
var importedSpriteCount = 0;
foreach (var importedAsset in importedAssets)
{
    if (importedAsset is UnityEngine.Sprite)
    {
        importedSpriteCount++;
    }
}
if (importedSpriteCount != spriteNames.Length)
{
    throw new System.Exception("Expected six imported life sprites but found " + importedSpriteCount);
}

UnityEngine.Debug.Log("Configured six full-resolution life sprites: " + string.Join(", ", spriteNames));
