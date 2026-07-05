# -*- coding: utf-8 -*-
# TutorialPopup: Page4/5 삭제 + Page3을 '게임의 흐름 + 조작법' 스프레드로 재구축
import io, re

p='Assets/Resources/UI/Popup/TutorialPopup.prefab'
s=io.open(p,encoding='utf-8').read()

# TMP 블록 템플릿 (Page2 TitleText 복제용) — 삭제 전에 확보
tmp_src = re.search(r'--- !u!114 &9120100000000004\nMonoBehaviour:.*?(?=--- !u!)', s, re.S).group(0)

# ── 기존 913x/914x/915x 블록 제거
head=s[:s.index('--- !u!')]
idx=[m.start() for m in re.finditer(r'^--- !u!\d+ &(-?\d+)', s, re.M)]+[len(s)]
kept=[]
for i in range(len(idx)-1):
    b=s[idx[i]:idx[i+1]]
    bid=re.match(r'--- !u!\d+ &(\d+)', b).group(1)
    if bid.startswith(('913','914','915')): continue
    kept.append(b)
s=head+''.join(kept)

old='''  - {fileID: 9110000000000002}
  - {fileID: 9120000000000002}
  - {fileID: 9130000000000002}
  - {fileID: 9140000000000002}
  - {fileID: 9150000000000002}'''
assert s.count(old)==1
s=s.replace(old,'''  - {fileID: 9110000000000002}
  - {fileID: 9120000000000002}
  - {fileID: 9930000000000002}''')

IMG='fe87c0e1cc204ed48ad3b37840f39efc'
LINE='3c8b5e2a9d6f4137b0a4c7e1d8f36925'
G={'timeline':'a1c47e9b2d5f4680b3e8c1d6a9f24073','keyA':'b2d58fa03e604791c4f9d2e7b0a35184',
   'keyW':'c3e69ab14f7148a2d50ae3f8c1b46295','keyS':'d4f70bc2508159b3e61bf409d2c573a6',
   'keyD':'e5081cd361926ac4f72c051ae3d684b7','keyF':'f6192de472a37bd50834162bf4e795c8',
   'mouse':'071a3ef583b48ce61945273c05f8a6d9','space':'182b4f0694c59df72a56384d16a9b7ea'}

out=[]
children=[]
seq=[10]
PAGE_RECT='9930000000000002'

def nid():
    seq[0]+=1
    return '99%02d000000000000'%seq[0]

def widget_img(name,pos,size,guid,preserve=1):
    b=nid(); gid=b[:-1]+'1'; rid=b[:-1]+'2'; cid=b[:-1]+'3'; mid=b[:-1]+'4'
    out.append('--- !u!1 &%s\nGameObject:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n  - component: {fileID: %s}\n  - component: {fileID: %s}\n  - component: {fileID: %s}\n  m_Layer: 5\n  m_Name: %s\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n'%(gid,rid,cid,mid,name))
    out.append('--- !u!224 &%s\nRectTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %s}\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children: []\n  m_Father: {fileID: %s}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0.5, y: 0.5}\n  m_AnchorMax: {x: 0.5, y: 0.5}\n  m_AnchoredPosition: {x: %s, y: %s}\n  m_SizeDelta: {x: %s, y: %s}\n  m_Pivot: {x: 0.5, y: 0.5}\n'%(rid,gid,PAGE_RECT,pos[0],pos[1],size[0],size[1]))
    out.append('--- !u!222 &%s\nCanvasRenderer:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %s}\n  m_CullTransparentMesh: 1\n'%(cid,gid))
    out.append('--- !u!114 &%s\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %s}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n  m_Name:\n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image\n  m_Material: {fileID: 0}\n  m_Color: {r: 1, g: 1, b: 1, a: 1}\n  m_RaycastTarget: 0\n  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n  m_Sprite: {fileID: 21300000, guid: %s, type: 3}\n  m_Type: 0\n  m_PreserveAspect: %d\n  m_FillCenter: 1\n  m_FillMethod: 4\n  m_FillAmount: 1\n  m_FillClockwise: 1\n  m_FillOrigin: 0\n  m_UseSpriteMesh: 0\n  m_PixelsPerUnitMultiplier: 1\n'%(mid,gid,IMG,guid,preserve))
    children.append(rid)

def widget_tmp(name,pos,size,text,fontsize):
    b=nid(); gid=b[:-1]+'1'; rid=b[:-1]+'2'; cid=b[:-1]+'3'; mid=b[:-1]+'4'
    out.append('--- !u!1 &%s\nGameObject:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n  - component: {fileID: %s}\n  - component: {fileID: %s}\n  - component: {fileID: %s}\n  m_Layer: 5\n  m_Name: %s\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n'%(gid,rid,cid,mid,name))
    out.append('--- !u!224 &%s\nRectTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %s}\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children: []\n  m_Father: {fileID: %s}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: {x: 0.5, y: 0.5}\n  m_AnchorMax: {x: 0.5, y: 0.5}\n  m_AnchoredPosition: {x: %s, y: %s}\n  m_SizeDelta: {x: %s, y: %s}\n  m_Pivot: {x: 0.5, y: 0.5}\n'%(rid,gid,PAGE_RECT,pos[0],pos[1],size[0],size[1]))
    out.append('--- !u!222 &%s\nCanvasRenderer:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: %s}\n  m_CullTransparentMesh: 1\n'%(cid,gid))
    tb = tmp_src.replace('9120100000000004', mid)
    tb = tb.replace('m_GameObject: {fileID: 9120100000000001}','m_GameObject: {fileID: %s}'%gid)
    tb = re.sub(r'm_text: ".*?"', lambda m: 'm_text: "%s"'%text.replace('\n','\\n'), tb, flags=re.S)
    tb = re.sub(r'm_fontSize: [\d.]+', 'm_fontSize: %d'%fontsize, tb)
    tb = re.sub(r'm_fontSizeBase: [\d.]+', 'm_fontSizeBase: %d'%fontsize, tb)
    out.append(tb)
    children.append(rid)

# ── 왼쪽: 게임의 흐름
widget_tmp('TitleText',(-358,255),(460,60),'3. 게임의 흐름',42)
widget_img('TitleLine',(-358,215),(250,8),LINE,preserve=0)
widget_tmp('FlowText',(-358,120),(560,160),
    "페이즈(3분) \\u2192 휴식(2분) 구조 반복\n페이즈 : '정령의 장난' 발동 O\n휴식 시간 : '정령의 장난' 발동 X",24)
widget_img('Timeline',(-358,-20),(560,56),G['timeline'],preserve=1)
widget_tmp('FlowText2',(-358,-140),(560,120),
    "각 페이즈의 목표 점수를 넘기지 못하면 게임 끝!\n\\u2192 3페이즈 이상 버텼을 시 성공",24)

# ── 오른쪽: 조작법
widget_tmp('TitleText2',(358,255),(460,60),'4. 조작법',42)
widget_img('TitleLine2',(358,215),(250,8),LINE,preserve=0)
K=58
widget_img('KeyW',(240,120),(K,K),G['keyW'])
widget_img('KeyA',(176,56),(K,K),G['keyA'])
widget_img('KeyS',(240,56),(K,K),G['keyS'])
widget_img('KeyD',(304,56),(K,K),G['keyD'])
widget_tmp('MoveLabel',(240,0),(200,30),'이동',22)
widget_img('Mouse',(500,88),(48,66),G['mouse'])
widget_tmp('MouseLabel',(500,30),(260,30),'공격 / UI 상호작용',22)
widget_img('KeyF',(240,-110),(K,K),G['keyF'])
widget_tmp('FLabel',(240,-170),(200,30),'상호작용',22)
widget_img('Space',(500,-110),(180,56),G['space'])
widget_tmp('SpaceLabel',(500,-170),(200,30),'점프',22)

# ── Page3 루트 (children 확정 후)
ch='\n'.join('  - {fileID: %s}'%c for c in children)
page_go='''--- !u!1 &9930000000000001
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 9930000000000002}
  m_Layer: 5
  m_Name: Page3
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!224 &9930000000000002
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 9930000000000001}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
%s
  m_Father: {fileID: 9020000000000002}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 0}
  m_AnchorMax: {x: 1, y: 1}
  m_AnchoredPosition: {x: 0, y: 0}
  m_SizeDelta: {x: 0, y: 0}
  m_Pivot: {x: 0.5, y: 0.5}
''' % ch

s = s + page_go + ''.join(out)
io.open(p,'w',encoding='utf-8',newline='\n').write(s)
ids=re.findall(r'--- !u!\d+ &(-?\d+)', s)
assert len(ids)==len(set(ids)), 'dup fileID'
print('page3 rebuilt,', len(ids), 'objects total')
